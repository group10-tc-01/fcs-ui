using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.DTOs;

[ExcludeFromCodeCoverage]
public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Cpf,
    string Role
);
