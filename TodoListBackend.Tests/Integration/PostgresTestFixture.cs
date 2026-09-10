using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoListBackend.Data;
using Xunit;

namespace TodoListBackend.Tests.Integration;

public sealed class PostgresTestFixture : IAsyncLifetime
{
    private const int PostgresPort = 5432;
    private IContainer? _container;

    public TodoListWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        if (!IsEnabled)
        {
            return;
        }

        _container = new ContainerBuilder("postgres:16-alpine")
            .WithEnvironment("POSTGRES_DB", "todolist_test")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithPortBinding(PostgresPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(
                "database system is ready to accept connections"))
            .Build();

        await _container.StartAsync();
        var connectionString =
            $"Host={_container.Hostname};Port={_container.GetMappedPublicPort(PostgresPort)};" +
            "Database=todolist_test;Username=postgres;Password=postgres;Include Error Detail=true";

        Factory = new TodoListWebApplicationFactory(connectionString);
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        Factory?.Dispose();
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    public static bool IsEnabled =>
        string.Equals(
            Environment.GetEnvironmentVariable("RUN_POSTGRES_INTEGRATION_TESTS"),
            "1",
            StringComparison.Ordinal);
}

public sealed class TodoListWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TodoListWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Jwt:Key"] = "integration-test-signing-key-with-at-least-32-bytes",
                ["Jwt:Issuer"] = "TodoList.Tests",
                ["Jwt:Audience"] = "TodoList.Tests.Client",
                ["Jwt:AccessTokenMinutes"] = "10",
                ["CloudinarySettings:CloudName"] = "integration-test",
                ["CloudinarySettings:ApiKey"] = "integration-test",
                ["CloudinarySettings:ApiSecret"] = "integration-test",
                ["RefreshToken:ExpiryDays"] = "7",
                ["RefreshToken:CookieName"] = "todo_refresh_token_test",
                ["RefreshToken:CookiePath"] = "/api/auth",
                ["RefreshToken:SameSite"] = "Lax",
                ["RefreshToken:Secure"] = "false",
                ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
            });
        });
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<PostgresTestFixture>
{
    public const string Name = "PostgreSQL integration tests";
}

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (!PostgresTestFixture.IsEnabled)
        {
            Skip = "Set RUN_POSTGRES_INTEGRATION_TESTS=1 and start Docker to run PostgreSQL integration tests.";
        }
    }
}

public abstract class IntegrationTestBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    protected readonly PostgresTestFixture Fixture;

    protected IntegrationTestBase(PostgresTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected HttpClient CreateClient(string accessToken)
    {
        if (!PostgresTestFixture.IsEnabled)
        {
            throw new InvalidOperationException(
                "PostgreSQL integration tests are opt-in. Set RUN_POSTGRES_INTEGRATION_TESTS=1.");
        }

        var client = Fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    protected static async Task<AuthSession> RegisterAsync(HttpClient client, string suffix)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"owner_{suffix}",
            email = $"owner_{suffix}@example.test",
            password = "Password123!"
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Register response was empty.");
        return new AuthSession(payload.AccessToken, payload.User?.Id
            ?? throw new InvalidOperationException("Register response did not include a user id."));
    }

    protected static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions)
            ?? throw new InvalidOperationException("Response body was empty.");
    }

    protected sealed record AuthSession(string AccessToken, int UserId);

    private sealed class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public UserResponse? User { get; set; }
    }

    private sealed class UserResponse
    {
        public int Id { get; set; }
    }
}
