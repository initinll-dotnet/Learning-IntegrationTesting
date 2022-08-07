using Customers.Api;
using Customers.Api.Database;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;

using Xunit;

namespace Customer.Api.Tests.Integration;

public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    public const string ValidGithubUser = "validuser";

    // option 1
    //private readonly TestcontainersContainer _dbContainer =
    //    new TestcontainersBuilder<TestcontainersContainer>()
    //    .WithImage("postgres:latest")
    //    .WithEnvironment("POSTGRES_USER", "course")
    //    .WithEnvironment("POSTGRES_PASSWORD", "changeme")
    //    .WithEnvironment("POSTGRES_DB", "mydb")
    //    .WithPortBinding(5555, 5432)
    //    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
    //    .Build();

    // option 2
    private readonly TestcontainerDatabase _dbContainer =
        new TestcontainersBuilder<PostgreSqlTestcontainer>()
        .WithDatabase(new PostgreSqlTestcontainerConfiguration
        {
            Database = "db",
            Username = "course",
            Password = "changeme"
        }).Build();

    private readonly GitHubApiServer _gitHubApiServer = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services => 
        {
            services.RemoveAll(typeof(IDbConnectionFactory));

            // option 1
            //services.TryAddSingleton<IDbConnectionFactory>(_ =>
            //new NpgsqlConnectionFactory("Server=localhost;Port=5555;Database=mydb;User ID=course;Password=changeme;"));

            // option 2
            services.TryAddSingleton<IDbConnectionFactory>(_ =>
            new NpgsqlConnectionFactory(_dbContainer.ConnectionString));

            services.AddHttpClient("GitHub", httpClient =>
            {
                httpClient.BaseAddress = new Uri(_gitHubApiServer.Url);
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.UserAgent, $"Course-{Environment.MachineName}");
            });
        });
    }

    public async Task InitializeAsync()
    {
        _gitHubApiServer.Start();
        _gitHubApiServer.SetupUser(ValidGithubUser);
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        _gitHubApiServer.Dispose();
    }
}
