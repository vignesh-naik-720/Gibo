namespace Gibo.Domain.Exceptions;

/// <summary>
/// Thrown when a business invariant or ethical rule in the Gibo domain is violated.
/// </summary>
public sealed class DomainValidationException : Exception
{
    public string ErrorCode { get; }

    public DomainValidationException(string message, string errorCode = "DOMAIN_VALIDATION_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
