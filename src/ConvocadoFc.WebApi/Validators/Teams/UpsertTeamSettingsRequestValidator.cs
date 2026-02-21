using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class UpsertTeamSettingsRequestValidator : AbstractValidator<UpsertTeamSettingsRequest>
{
    public UpsertTeamSettingsRequestValidator()
    {
        RuleForEach(request => request.Settings)
            .SetValidator(new UpsertTeamSettingEntryRequestValidator());
    }
}

public sealed class UpsertTeamSettingEntryRequestValidator : AbstractValidator<UpsertTeamSettingEntryRequest>
{
    public UpsertTeamSettingEntryRequestValidator()
    {
        RuleFor(entry => entry.Key)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(entry => entry.Value)
            .NotEmpty()
            .MaximumLength(1200);

        RuleFor(entry => entry.ValueType)
            .MaximumLength(40)
            .When(entry => !string.IsNullOrWhiteSpace(entry.ValueType));

        RuleFor(entry => entry.Description)
            .MaximumLength(300)
            .When(entry => !string.IsNullOrWhiteSpace(entry.Description));
    }
}
