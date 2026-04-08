using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class NewCategoryInputModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, ErrorMessage = "Name must be 50 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Color is required.")]
    [StringLength(7, ErrorMessage = "Color must be a valid hex color (e.g. #FF5733).")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g. #FF5733).")]
    public string Color { get; set; } = "#3B82F6";
}
