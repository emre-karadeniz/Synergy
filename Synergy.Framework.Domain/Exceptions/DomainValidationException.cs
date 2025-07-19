namespace Synergy.Framework.Domain.Exceptions;

public class DomainValidationException: Exception
{
    public IReadOnlyList<string> Errors { get; }
    public DomainValidationException(IEnumerable<string> errors) : base("Domain validation error!")
    {
        Errors = errors.ToList();
    }
}
