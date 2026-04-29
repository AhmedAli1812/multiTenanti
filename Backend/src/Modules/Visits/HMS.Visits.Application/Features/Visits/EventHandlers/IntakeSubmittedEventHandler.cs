using HMS.Rooms.Domain.Entities;
using HMS.Visits.Application.Abstractions;
using HMS.Visits.Domain.Entities;
using HMS.SharedKernel.Primitives;
using HMS.Intake.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
// VisitType alias — Intake and Visits both define VisitType with same int values
using IntakeVisitType = HMS.Intake.Domain.Entities.VisitType;

namespace HMS.Visits.Application.Features.Visits.EventHandlers;

/// <summary>
/// Cross-module event handler: Intake → Visits.
///
/// When an intake is submitted, this handler creates the Visit and assigns
/// a Room and Doctor. The Intake module knows nothing about Visits.
/// </summary>
public sealed class IntakeSubmittedEventHandler(
    IVisitsDbContext context)
    : INotificationHandler<IntakeSubmittedEvent>
{
    public async Task Handle(IntakeSubmittedEvent evt, CancellationToken cancellationToken)
    {
        await using var transaction = await context.BeginTransactionAsync(cancellationToken);

        try
        {
            // ── Assign Room ────────────────────────────────────────────────────
            Guid  roomId;
            Guid? doctorId;
            (roomId, doctorId) = await AssignRoomAndDoctorAsync(
                evt.TenantId, evt.BranchId, cancellationToken);

            // ── Queue number ───────────────────────────────────────────────────
            var queueNumber = await GenerateQueueNumberAsync(
                evt.BranchId, evt.TenantId, cancellationToken);

            // ── Map Intake VisitType → Visits VisitType (same int values) ──────
            var visitType = (VisitType)(int)evt.VisitType;

            // ── Create Visit ───────────────────────────────────────────────────
            var visit = Visit.Create(
                patientId:   evt.PatientId,
                branchId:    evt.BranchId,
                visitType:   visitType,
                tenantId:    evt.TenantId,
                queueNumber: queueNumber,
                doctorId:    doctorId,
                createdBy:   null);

            await context.Visits.AddAsync(visit, cancellationToken);

            // ── Create RoomAssignment ──────────────────────────────────────────
            var assignment = RoomAssignment.Create(
                visitId:   visit.Id,
                roomId:    roomId,
                tenantId:  evt.TenantId,
                createdBy: Guid.Empty);

            await context.RoomAssignments.AddAsync(assignment, cancellationToken);

            // ── Mark Intake as ConvertedToVisit ────────────────────────────────
            var intake = await context.Intakes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.Id == evt.IntakeId, cancellationToken);

            intake?.MarkConvertedToVisit(visit.Id);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            // Transaction auto-rolls back on dispose without commit
            throw;
        }
    }

    private async Task<(Guid roomId, Guid? doctorId)> AssignRoomAndDoctorAsync(
        Guid tenantId,
        Guid branchId,
        CancellationToken ct)
    {
        // UPDLOCK via raw SQL prevents double-booking race conditions
        var room = await context.Rooms
            .FromSqlRaw("""
                SELECT TOP 1 r.* FROM rooms.Rooms r WITH (UPDLOCK, ROWLOCK)
                WHERE r.TenantId = {0}
                  AND r.BranchId = {1}
                  AND r.IsOccupied = 0
                  AND r.IsDeleted  = 0
                  AND (r.CleaningUntil IS NULL OR r.CleaningUntil <= GETUTCDATE())
                ORDER BY r.RoomNumber
                """,
                tenantId, branchId)
            .FirstOrDefaultAsync(ct)
            ?? throw new ConflictException(
                $"No available room found in branch '{branchId}' for tenant '{tenantId}'.");

        room.Assign();

        // Least-loaded doctor — ordered by active visit count
        var doctor = await context.Users
            .AsNoTracking()
            .Where(u =>
                u.TenantId  == tenantId &&
                u.BranchId  == branchId &&
                u.IsActive   &&
                !u.IsDeleted &&
                u.UserRoles.Any(ur => ur.Role.Name == "Doctor"))
            .OrderBy(u =>
                context.Visits.Count(v =>
                    v.DoctorId == u.Id &&
                    v.Status   != VisitStatus.Completed &&
                    v.TenantId == tenantId))
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(ct);

        return (room.Id, doctor);
    }

    private async Task<int> GenerateQueueNumberAsync(
        Guid branchId, Guid tenantId, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var last  = await context.Visits
            .IgnoreQueryFilters()
            .Where(v =>
                v.TenantId  == tenantId &&
                v.BranchId  == branchId &&
                v.VisitDate >= today    &&
                !v.IsDeleted)
            .OrderByDescending(v => v.QueueNumber)
            .Select(v => (int?)v.QueueNumber)
            .FirstOrDefaultAsync(ct);

        return (last ?? 0) + 1;
    }
}
