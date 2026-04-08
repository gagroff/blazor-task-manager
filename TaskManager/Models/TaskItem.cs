using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }
}
