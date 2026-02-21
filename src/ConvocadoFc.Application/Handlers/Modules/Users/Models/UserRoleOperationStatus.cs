namespace ConvocadoFc.Application.Handlers.Modules.Users.Models;

public enum EUserRoleOperationStatus
{
    Success = 0,
    InvalidRole = 1,
    Forbidden = 2,
    UserNotFound = 3,
    RoleNotConfigured = 4,
    Failed = 5
}
