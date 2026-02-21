using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class TeamInvite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? TargetUserId { get; set; }
    public string? TargetEmail { get; set; }
    public string Token { get; set; } = string.Empty;
    public ETeamInviteChannel Channel { get; set; } = ETeamInviteChannel.Email;
    public ETeamInviteStatus Status { get; set; } = ETeamInviteStatus.Pending;
    public bool IsPreApproved { get; set; }
    public int? MaxUses { get; set; }
    public int UseCount { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }

    public Team? Team { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public ApplicationUser? TargetUser { get; set; }
}
