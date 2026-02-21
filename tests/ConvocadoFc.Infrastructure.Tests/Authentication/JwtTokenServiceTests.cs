using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Infrastructure.Modules.Authentication;
using Microsoft.Extensions.Options;

namespace ConvocadoFc.Infrastructure.Tests.Authentication;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_IncludesClaimsAndRoles()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "issuer",
            Audience = "audience",
            SigningKey = "super-secret-signing-key-1234567890",
            AccessTokenMinutes = 30
        });

        var service = new JwtTokenService(options);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "user@local",
            FullName = "User",
            EmailConfirmed = true
        };

        var token = service.CreateToken(user, new[] { "Admin", "User" });
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("issuer", jwt.Issuer);
        Assert.Equal("audience", jwt.Audiences.First());
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == "user@local");
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Name && claim.Value == "User");
        Assert.Contains(jwt.Claims, claim => claim.Type == "email_confirmed" && claim.Value == "true");
        Assert.Equal(2, jwt.Claims.Count(claim => claim.Type == ClaimTypes.Role));
    }
}
