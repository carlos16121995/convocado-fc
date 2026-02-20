using System.Collections.Generic;

namespace ConvocadoFc.Domain.Modules.Users.Identity;

public static class SystemRoles
{
    public const string Master = "Master";
    public const string Admin = "Admin";
    public const string Moderator = "Moderador";
    public const string User = "Usuario";

    public const string AdminOrMaster = Admin + "," + Master;
    public const string ModeratorAdminMaster = Moderator + "," + Admin + "," + Master;

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        User,
        Moderator,
        Admin,
        Master
    };
}
