using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HomeFieldName { get; set; } = string.Empty;
    public string? HomeFieldAddress { get; set; }
    public decimal? HomeFieldLatitude { get; set; }
    public decimal? HomeFieldLongitude { get; set; }
    public string? CrestUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public ApplicationUser? OwnerUser { get; set; }
    public TeamSettings? Settings { get; set; }
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TeamInvite> Invites { get; set; } = new List<TeamInvite>();
    public ICollection<TeamJoinRequest> JoinRequests { get; set; } = new List<TeamJoinRequest>();
}
