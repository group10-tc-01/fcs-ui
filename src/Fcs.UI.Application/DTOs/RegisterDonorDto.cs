using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.DTOs;

[ExcludeFromCodeCoverage]
public sealed record RegisterDonorDto(
    Guid Id,
    string FullName,
    string Email,
    string Cpf);
