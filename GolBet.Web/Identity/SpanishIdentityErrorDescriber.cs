using Microsoft.AspNetCore.Identity;

namespace GolBet.Web.Identity;

public sealed class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };

    public override IdentityError DefaultError() => Error(nameof(DefaultError), "No se pudo completar la operación. Inténtalo de nuevo.");
    public override IdentityError ConcurrencyFailure() => Error(nameof(ConcurrencyFailure), "Los datos cambiaron. Recarga la página e inténtalo de nuevo.");
    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), "Ya existe una cuenta con ese correo electrónico.");
    public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), "Ya existe una cuenta con ese correo electrónico.");
    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), "Escribe un correo electrónico válido.");
    public override IdentityError InvalidUserName(string? userName) => Error(nameof(InvalidUserName), "El correo contiene caracteres no permitidos.");
    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"La contraseña debe tener al menos {length} caracteres.");
    public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "La contraseña debe incluir al menos un número.");
    public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "La contraseña debe incluir al menos una letra minúscula.");
    public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "La contraseña debe incluir al menos una letra mayúscula.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "La contraseña debe incluir al menos un símbolo.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error(nameof(PasswordRequiresUniqueChars), $"La contraseña debe tener al menos {uniqueChars} caracteres distintos.");
    public override IdentityError PasswordMismatch() => Error(nameof(PasswordMismatch), "La contraseña no es correcta.");
    public override IdentityError UserAlreadyInRole(string role) => Error(nameof(UserAlreadyInRole), "La cuenta ya tiene asignado ese rol.");
    public override IdentityError UserNotInRole(string role) => Error(nameof(UserNotInRole), "La cuenta no tiene asignado ese rol.");
    public override IdentityError InvalidRoleName(string? role) => Error(nameof(InvalidRoleName), "El nombre del rol no es válido.");
    public override IdentityError DuplicateRoleName(string role) => Error(nameof(DuplicateRoleName), "Ese rol ya existe.");
}
