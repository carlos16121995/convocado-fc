using Microsoft.AspNetCore.Identity;

using System;

namespace ConvocadoFc.Domain.Models.Modules.Users.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
