using HMS.SharedKernel.Application.Abstractions;

namespace HMS.Identity.Application.Features.Auth.Login;

public sealed record LoginCommand(
    string Identifier,  // email, phone, or username
    string Password,
    string? DeviceId = null
) : ICommand<LoginResponse>;

public sealed record LoginResponse(
    string  AccessToken,
    string  RefreshToken,
    string  FullName,
    string  Role,
    Guid    UserId,
    Guid?   TenantId,
    Guid?   BranchId,
    string? Email
);
