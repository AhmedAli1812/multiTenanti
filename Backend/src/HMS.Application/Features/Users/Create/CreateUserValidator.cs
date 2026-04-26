using FluentValidation;

namespace HMS.Application.Features.Users.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    private readonly Guid _doctorRoleId;

    public CreateUserValidator(Guid doctorRoleId)
    {
        _doctorRoleId = doctorRoleId;

        // =========================
        // 🔹 Basic Validation
        // =========================
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.RoleIds)
            .NotEmpty()
            .WithMessage("At least one role must be selected");

        // =========================
        // 💣 Doctor Validation
        // =========================
        When(x => IsDoctor(x), () =>
        {
            RuleFor(x => x.DepartmentId)
                .NotNull()
                .WithMessage("Doctor must have a department");

            RuleFor(x => x.BranchId)
                .NotNull()
                .WithMessage("Doctor must have a branch");
        });
    }

    // =========================
    // 🔥 Helper
    // =========================
    private bool IsDoctor(CreateUserCommand command)
    {
        return command.RoleIds != null &&
               command.RoleIds.Contains(_doctorRoleId);
    }
}