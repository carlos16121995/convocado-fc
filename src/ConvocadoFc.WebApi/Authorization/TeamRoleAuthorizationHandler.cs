using System.Security.Claims;

using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.WebApi.Authorization;

public sealed class TeamRoleAuthorizationHandler(IApplicationDbContext dbContext) : AuthorizationHandler<TeamRoleRequirement>
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TeamRoleRequirement requirement)
    {
        if (!TryGetUserId(context, out var userId))
        {
            return;
        }

        if (context.User.IsInRole(SystemRoles.Master) || context.User.IsInRole(SystemRoles.Admin))
        {
            context.Succeed(requirement);
            return;
        }

        var teamId = await ResolveTeamIdAsync(context);
        if (!teamId.HasValue)
        {
            return;
        }

        var hasRole = await _dbContext.Query<TeamMember>()
            .AnyAsync(member => member.TeamId == teamId.Value
                                && member.UserId == userId
                                && member.Status == ETeamMemberStatus.Active
                                && requirement.AllowedRoles.Contains(member.Role));

        if (hasRole)
        {
            context.Succeed(requirement);
        }
    }

    private static bool TryGetUserId(AuthorizationHandlerContext context, out Guid userId)
    {
        userId = Guid.Empty;
        var rawId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return rawId is not null && Guid.TryParse(rawId, out userId);
    }

    private async Task<Guid?> ResolveTeamIdAsync(AuthorizationHandlerContext context)
    {
        if (context.Resource is not AuthorizationFilterContext mvcContext)
        {
            return null;
        }

        if (mvcContext.RouteData.Values.TryGetValue("teamId", out var routeValue)
            && Guid.TryParse(routeValue?.ToString(), out var teamId))
        {
            return teamId;
        }

        if (mvcContext.HttpContext.Request.Query.TryGetValue("teamId", out var queryValue)
            && Guid.TryParse(queryValue.ToString(), out var queryTeamId))
        {
            return queryTeamId;
        }

        if (mvcContext.RouteData.Values.TryGetValue("ruleId", out var ruleValue)
            && Guid.TryParse(ruleValue?.ToString(), out var ruleId))
        {
            return await ResolveTeamIdFromRuleAsync(ruleId);
        }

        if (mvcContext.RouteData.Values.TryGetValue("parameterId", out var parameterValue)
            && Guid.TryParse(parameterValue?.ToString(), out var parameterId))
        {
            return await ResolveTeamIdFromParameterAsync(parameterId);
        }

        return null;
    }

    private async Task<Guid?> ResolveTeamIdFromRuleAsync(Guid ruleId)
        => await (from rule in _dbContext.Query<TeamRule>()
                  join settings in _dbContext.Query<TeamSettings>() on rule.TeamSettingsId equals settings.Id
                  where rule.Id == ruleId
                  select (Guid?)settings.TeamId)
            .FirstOrDefaultAsync();

    private async Task<Guid?> ResolveTeamIdFromParameterAsync(Guid parameterId)
        => await (from parameter in _dbContext.Query<TeamRuleParameter>()
                  join rule in _dbContext.Query<TeamRule>() on parameter.TeamRuleId equals rule.Id
                  join settings in _dbContext.Query<TeamSettings>() on rule.TeamSettingsId equals settings.Id
                  where parameter.Id == parameterId
                  select (Guid?)settings.TeamId)
            .FirstOrDefaultAsync();
}
