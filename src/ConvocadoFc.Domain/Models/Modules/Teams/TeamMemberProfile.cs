namespace ConvocadoFc.Domain.Models.Modules.Teams;

public sealed class TeamMemberProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeamMemberId { get; set; }
    public Guid? CopiedFromTeamId { get; set; }
    public bool IsFeeExempt { get; set; }
    public bool IsOnHiatus { get; set; }
    public DateTimeOffset? HiatusStartedAt { get; set; }
    public DateTimeOffset? HiatusEndsAt { get; set; }
    public int HiatusCountLast6Months { get; set; }
    public DateTimeOffset? LastHiatusStartedAt { get; set; }
    public EPlayerPosition? PrimaryPosition { get; set; }
    public EPlayerPosition? SecondaryPosition { get; set; }
    public EPlayerPosition? TertiaryPosition { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public TeamMember? TeamMember { get; set; }
}
