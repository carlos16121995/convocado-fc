namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public enum EAuthOperationStatus
{
    Success = 0,
    InvalidCredentials = 1,
    UserNotFound = 2,
    InvalidData = 3,
    InvalidToken = 4,
    RefreshTokenMissing = 5,
    RefreshTokenInvalid = 6,
    RequiresPhone = 7,
    Failed = 8
}
