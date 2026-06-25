using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Application.DTOs;

[ExcludeFromCodeCoverage]
public sealed record AuthSessionDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType);
