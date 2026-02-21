namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record TeamSettingsOperationResult(
    ETeamSettingsOperationStatus Status,
    TeamSettingsDto? Settings
);
