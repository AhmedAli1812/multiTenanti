namespace HMS.SharedKernel.Primitives;

/// <summary>
/// Base class for domain-specific exceptions.
/// These are EXPECTED errors (invariant violations, business rule failures).
/// Do NOT use for infrastructure errors (DB timeouts, network issues).
/// </summary>
public class DomainException : Exception
{
    public string? Code { get; }

    public DomainException(string message, string? code = null)
        : base(message)
    {
        Code = code;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object id)
        : base($"{entity} with id '{id}' was not found.", "NOT_FOUND") { }
}

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = errors.AsReadOnly();
    }
}

public class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(message, "CONFLICT") { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized.")
        : base(message, "UNAUTHORIZED") { }
}
