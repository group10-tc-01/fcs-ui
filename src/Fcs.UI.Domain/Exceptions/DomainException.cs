namespace Fcs.UI.Domain.Exceptions;

public sealed class DomainException(string message) : Exception(message)
{
}
