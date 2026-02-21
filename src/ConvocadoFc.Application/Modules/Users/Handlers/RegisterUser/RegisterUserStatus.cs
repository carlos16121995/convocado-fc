namespace ConvocadoFc.Application.Modules.Users.Handlers.RegisterUser;

/// <summary>
/// Resultado da operação de cadastro de usuário.
/// </summary>
public enum ERegisterUserStatus
{
    /// <summary>
    /// Cadastro realizado com sucesso.
    /// </summary>
    Success,
    /// <summary>
    /// E-mail já está em uso.
    /// </summary>
    EmailAlreadyExists,
    /// <summary>
    /// Falha ao cadastrar o usuário.
    /// </summary>
    Failed
}
