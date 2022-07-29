using Customers.Api;
using Customers.Api.Database;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit;

namespace Customer.Api.Tests.Integration;

public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly TestcontainersContainer _dbContainer =
        new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage("postgres:latest")
        .WithEnvironment("POSTGRES_USER", "course")
        .WithEnvironment("POSTGRES_PASSWORD", "changeme")
        .WithEnvironment("POSTGRES_DB", "mydb")
        .WithPortBinding(5555, 5432)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services => 
        {
            services.RemoveAll(typeof(IDbConnectionFactory));
            services.TryAddSingleton<IDbConnectionFactory>(_ =>
            new NpgsqlConnectionFactory("Server=localhost;Port=5555;Database=mydb;User ID=course;Password=changeme;"));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
