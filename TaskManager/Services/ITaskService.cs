using TaskManager.Models;

namespace TaskManager.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync();
    Task<TaskItem> AddAsync(NewTaskInputModel input);
    Task<bool> MarkCompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}
