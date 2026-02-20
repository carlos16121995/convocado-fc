namespace ConvocadoFc.Application.Modules.Authentication.Abstractions;

public static class AuthConstants
{
    public const string PasswordRegex = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$";
    public const string EmailConfirmedClaim = "email_confirmed";
}
