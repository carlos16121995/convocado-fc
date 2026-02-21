using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class TeamJoinRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public Guid? InviteId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public ETeamJoinRequestStatus Status { get; set; } = ETeamJoinRequestStatus.Pending;
    public ETeamJoinRequestSource Source { get; set; } = ETeamJoinRequestSource.ProximitySearch;
    public bool IsAutoApproved { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReviewedAt { get; set; }

    public Team? Team { get; set; }
    public ApplicationUser? User { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }
    public TeamInvite? Invite { get; set; }
}
