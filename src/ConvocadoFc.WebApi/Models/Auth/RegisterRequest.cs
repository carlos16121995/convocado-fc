namespace ConvocadoFc.WebApi.Models.Auth;

/// <summary>
/// Solicitação de cadastro de usuário (legado).
/// </summary>
/// <param name="Name">Nome completo do usuário.</param>
/// <param name="Email">E-mail do usuário.</param>
/// <param name="Phone">Telefone do usuário.</param>
/// <param name="Password">Senha de acesso.</param>
/// <param name="Address">Endereço do usuário.</param>
/// <param name="ProfilePhotoUrl">URL da foto de perfil.</param>
public sealed record RegisterRequest(
    string Name,
    string Email,
    string Phone,
    string Password,
    string? Address,
    string? ProfilePhotoUrl
);
