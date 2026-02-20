using ConvocadoFc.Application.Handlers.Modules.Shared.Interfaces;
using Microsoft.Extensions.Options;

namespace ConvocadoFc.WebApi.Options;

public sealed class AppUrlProvider(IOptions<AppUrlOptions> options) : IAppUrlProvider
{
    private readonly AppUrlOptions _options = options.Value;

    public string ApiBaseUrl => _options.ApiBaseUrl;
    public string WebBaseUrl => _options.WebBaseUrl;
}
