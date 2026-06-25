namespace Fcs.UI.Application.Interfaces;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsInRole(string role);
    Task InitializeAsync();
}
