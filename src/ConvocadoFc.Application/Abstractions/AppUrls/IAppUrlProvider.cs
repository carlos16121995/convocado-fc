namespace ConvocadoFc.Application.Abstractions.AppUrls;

public interface IAppUrlProvider
{
    string ApiBaseUrl { get; }
    string WebBaseUrl { get; }
}
