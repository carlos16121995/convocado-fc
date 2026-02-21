using System;
using System.Collections.Generic;

namespace ConvocadoFc.WebApi.Models.Auth;

/// <summary>
/// Dados do usuário autenticado retornados após login (legado).
/// </summary>
/// <param name="UserId">Identificador do usuário.</param>
/// <param name="Email">E-mail do usuário.</param>
/// <param name="Name">Nome completo do usuário.</param>
/// <param name="EmailConfirmed">Indica se o e-mail foi confirmado.</param>
/// <param name="Roles">Lista de roles atribuídas ao usuário.</param>
public sealed record AuthUserResponse(
    Guid UserId,
    string Email,
    string Name,
    bool EmailConfirmed,
    IReadOnlyCollection<string> Roles
);
