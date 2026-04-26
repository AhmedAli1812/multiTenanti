using HMS.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Auth.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public LogoutHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // 💣 Validation
        if (request.UserId == Guid.Empty)
            throw new ArgumentException("Invalid user");

        // =========================
        // 🔥 Revoke Refresh Tokens
        // =========================
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == request.UserId && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        if (tokens.Count > 0)
        {
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
        }

        // =========================
        // 🔥 Remove Sessions (IMPORTANT)
        // =========================
        var sessions = await _context.UserSessions
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (sessions.Count > 0)
        {
            _context.UserSessions.RemoveRange(sessions);
        }

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}