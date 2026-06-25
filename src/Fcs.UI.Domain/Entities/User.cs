using Fcs.UI.Domain.Enums;
using Fcs.UI.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace Fcs.UI.Domain.Entities;

[ExcludeFromCodeCoverage]
public sealed class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Cpf Cpf { get; private set; }
    public UserRole Role { get; private set; }

    public User(string name, Email email, Cpf cpf, UserRole role)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Cpf = cpf;
        Role = role;
    }
}
