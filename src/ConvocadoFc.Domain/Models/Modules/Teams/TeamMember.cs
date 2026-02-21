using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class TeamMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public Guid? AddedByUserId { get; set; }
    public ETeamMemberRole Role { get; set; } = ETeamMemberRole.User;
    public ETeamMemberStatus Status { get; set; } = ETeamMemberStatus.Active;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public Team? Team { get; set; }
    public ApplicationUser? User { get; set; }
    public ApplicationUser? AddedByUser { get; set; }
    public TeamMemberProfile? Profile { get; set; }
}
