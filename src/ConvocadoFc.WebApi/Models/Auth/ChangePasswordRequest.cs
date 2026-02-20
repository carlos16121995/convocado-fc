namespace ConvocadoFc.WebApi.Models.Auth;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
