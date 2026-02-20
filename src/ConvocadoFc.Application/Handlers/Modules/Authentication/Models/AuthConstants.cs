namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public static class AuthConstants
{
    public const string PasswordRegex = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$";
    public const string EmailConfirmedClaim = "email_confirmed";
}
