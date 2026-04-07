using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class TaskService(AppDbContext dbContext) : ITaskService
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync()
    {
        return await dbContext.Tasks
            .AsNoTracking()
            .OrderBy(task => task.IsCompleted)
            .ThenBy(task => task.DueDate)
            .ThenBy(task => task.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<TaskItem> AddAsync(NewTaskInputModel input)
    {
        var task = new TaskItem
        {
            Title = input.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            DueDate = input.DueDate!.Value.Date,
            IsCompleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();

        return task;
    }

    public async Task<bool> MarkCompleteAsync(int id)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(item => item.Id == id);
        if (task is null)
        {
            return false;
        }

        if (task.IsCompleted)
        {
            return true;
        }

        task.IsCompleted = true;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await dbContext.Tasks.FirstOrDefaultAsync(item => item.Id == id);
        if (task is null)
        {
            return false;
        }

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
