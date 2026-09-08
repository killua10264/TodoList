using System.ComponentModel.DataAnnotations;

namespace TodoListBackend.DTOs.SubTask;

public sealed class SubTaskOrderItemDto
{
    [Range(1, int.MaxValue)]
    public int SubTaskId { get; set; }

    [Range(1, int.MaxValue)]
    public int SortOrder { get; set; }
}

public sealed class SubTaskOrderRequestDto
{
    [Required]
    public List<SubTaskOrderItemDto> Items { get; set; } = [];
}
