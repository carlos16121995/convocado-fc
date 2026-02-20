namespace ConvocadoFc.Application.Handlers.Modules.Shared.Interfaces;

public interface IAppUrlProvider
{
    string ApiBaseUrl { get; }
    string WebBaseUrl { get; }
}
