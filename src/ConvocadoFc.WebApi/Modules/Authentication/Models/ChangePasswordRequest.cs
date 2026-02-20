namespace ConvocadoFc.WebApi.Modules.Authentication.Models;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
