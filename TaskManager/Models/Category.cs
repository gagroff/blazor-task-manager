using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g. #FF5733).")]
    public string? Color { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = [];
}
