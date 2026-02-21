using ConvocadoFc.Application.Handlers.Modules.Notifications.Models;
using ConvocadoFc.Infrastructure.Modules.Notifications.Email;
using Microsoft.Extensions.Options;

namespace ConvocadoFc.Infrastructure.Tests.Notifications;

public sealed class EmailTemplateRendererTests
{
    [Fact]
    public async Task RenderAsync_WhenTemplatesExist_ReturnsMergedHtml()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var baseTemplate = "<html><body>{{Content}}</body></html>";
        var contentTemplate = "<h1>{{Title}}</h1><p>{{DynamicMessage}}</p><a href='{{ActionURL}}'>Link</a>";

        await File.WriteAllTextAsync(Path.Combine(tempDir, "base.html"), baseTemplate);
        await File.WriteAllTextAsync(Path.Combine(tempDir, "content.html"), contentTemplate);

        var renderer = new EmailTemplateRenderer(Options.Create(new EmailSettings
        {
            TemplatesPath = tempDir,
            BaseTemplateFile = "base.html",
            ContentTemplateFile = "content.html"
        }));

        var html = await renderer.RenderAsync(new EmailTemplateData("Title", "Message", "https://test"), CancellationToken.None);

        Assert.Contains("<h1>Title</h1>", html);
        Assert.Contains("Message", html);
        Assert.Contains("https://test", html);

        Directory.Delete(tempDir, true);
    }
}
