using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TodoListBackend.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class OwnershipIntegrationTests : IntegrationTestBase
{
    public OwnershipIntegrationTests(PostgresTestFixture fixture) : base(fixture)
    {
    }

    [PostgresFact]
    public async Task Second_user_cannot_read_or_mutate_first_users_todo_subtask_or_category()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        using var anonymousClient = Fixture.Factory.CreateClient();
        var owner = await RegisterAsync(anonymousClient, $"a{suffix}");
        var other = await RegisterAsync(anonymousClient, $"b{suffix}");
        using var ownerClient = CreateClient(owner.AccessToken);
        using var otherClient = CreateClient(other.AccessToken);

        var createCategory = await ownerClient.PostAsJsonAsync("/api/categories", new
        {
            name = $"Private {suffix}",
            color = "#16a085"
        });
        var category = await ReadJsonAsync<CategoryResponse>(createCategory);

        var createTodo = await ownerClient.PostAsJsonAsync("/api/todos", new
        {
            title = $"Private todo {suffix}",
            description = "Ownership test",
            priority = 2,
            dueDate = "2099-01-01",
            categoryId = category.Id
        });
        var todo = await ReadJsonAsync<TodoResponse>(createTodo);

        var createSubTask = await ownerClient.PostAsJsonAsync("/api/subtasks", new
        {
            title = "Private subtask",
            todoId = todo.Id
        });
        var subTask = await ReadJsonAsync<SubTaskResponse>(createSubTask);

        var list = await ReadJsonAsync<PaginatedResponse<TodoResponse>>(
            await otherClient.GetAsync("/api/todos?page=1&pageSize=20"));
        Assert.Empty(list.Items);

        var forbiddenReads = new[]
        {
            await otherClient.GetAsync($"/api/todos/{todo.Id}"),
            await otherClient.GetAsync($"/api/subtasks/{subTask.Id}"),
            await otherClient.GetAsync($"/api/subtasks/todo/{todo.Id}"),
            await otherClient.GetAsync($"/api/categories/{category.Id}")
        };

        Assert.All(forbiddenReads, response => Assert.Equal(HttpStatusCode.NotFound, response.StatusCode));

        var updateTodo = await otherClient.PutAsJsonAsync($"/api/todos/{todo.Id}", new
        {
            title = "Attempted takeover",
            description = "Should not persist",
            priority = 2,
            dueDate = "2099-01-01",
            categoryId = category.Id,
            isCompleted = true,
            version = todo.Version
        });
        Assert.Equal(HttpStatusCode.NotFound, updateTodo.StatusCode);

        var deleteTodo = await otherClient.DeleteAsync($"/api/todos/{todo.Id}?version={todo.Version}");
        Assert.Equal(HttpStatusCode.NotFound, deleteTodo.StatusCode);

        var restoreTodo = await otherClient.PostAsJsonAsync(
            $"/api/todos/{todo.Id}/restore?version={todo.Version}", new { });
        Assert.Equal(HttpStatusCode.NotFound, restoreTodo.StatusCode);

        var hardDeleteTodo = await otherClient.DeleteAsync($"/api/todos/{todo.Id}/hard?version={todo.Version}");
        Assert.Equal(HttpStatusCode.NotFound, hardDeleteTodo.StatusCode);

        var updateSubTask = await otherClient.PutAsJsonAsync($"/api/subtasks/{subTask.Id}", new
        {
            title = "Attempted takeover",
            isCompleted = true,
            version = subTask.Version
        });
        Assert.Equal(HttpStatusCode.NotFound, updateSubTask.StatusCode);

        var deleteSubTask = await otherClient.DeleteAsync($"/api/subtasks/{subTask.Id}?version={subTask.Version}");
        Assert.Equal(HttpStatusCode.NotFound, deleteSubTask.StatusCode);

        var updateCategory = await otherClient.PutAsJsonAsync($"/api/categories/{category.Id}", new
        {
            name = "Attempted takeover",
            color = "#c0392b"
        });
        Assert.Equal(HttpStatusCode.NotFound, updateCategory.StatusCode);

        var deleteCategory = await otherClient.DeleteAsync($"/api/categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteCategory.StatusCode);

        var ownerStillSeesTodo = await ownerClient.GetAsync($"/api/todos/{todo.Id}");
        Assert.Equal(HttpStatusCode.OK, ownerStillSeesTodo.StatusCode);
    }

    private sealed class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = [];
    }

    private sealed class CategoryResponse
    {
        public int Id { get; set; }
    }

    private sealed class TodoResponse
    {
        public int Id { get; set; }
        public uint Version { get; set; }
    }

    private sealed class SubTaskResponse
    {
        public int Id { get; set; }
        public uint Version { get; set; }
    }
}
