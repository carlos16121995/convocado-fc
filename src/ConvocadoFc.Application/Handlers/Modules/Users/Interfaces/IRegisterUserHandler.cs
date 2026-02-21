using ConvocadoFc.Application.Handlers.Modules.Users.Models;

namespace ConvocadoFc.Application.Handlers.Modules.Users.Interfaces;

public interface IRegisterUserHandler
{
    Task<RegisterUserResult> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken);
}
