using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class NewTaskInputModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title must be 100 characters or fewer.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Due date is required.")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; } = DateTime.Today;

    public int? CategoryId { get; set; }
}
