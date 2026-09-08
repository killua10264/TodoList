using System.ComponentModel.DataAnnotations;

namespace TodoListBackend.DTOs.Todo;

public enum TodoFilter
{
    Today,
    Upcoming
}

public enum TodoStatus
{
    All,
    Pending,
    Completed
}

public enum TodoSortBy
{
    DueDate,
    PriorityDesc,
    CreatedAt
}

public sealed class TodoQueryDto
{
    [Range(1, 1_000_000)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public TodoFilter? Filter { get; set; }
    public int? CategoryId { get; set; }
    public TodoStatus? Status { get; set; }
    public TodoSortBy? SortBy { get; set; }
    public bool? IsHidden { get; set; } = false;

    [StringLength(200)]
    public string? Search { get; set; }

    public bool? IsDeleted { get; set; } = false;
}
