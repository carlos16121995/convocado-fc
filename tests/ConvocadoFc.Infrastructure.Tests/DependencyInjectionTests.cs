using ConvocadoFc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConvocadoFc.Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_WhenRedisMissing_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
            })
            .Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddInfrastructure(config));

        Assert.Contains("Redis connection string is required", exception.Message);
    }
}
