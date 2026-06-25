using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.DTOs;

[ExcludeFromCodeCoverage]
public sealed class ApiEnvelope<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
}
