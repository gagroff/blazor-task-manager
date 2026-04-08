using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    [Fact]
    public async Task GetAllAsync_OrdersByOpenThenDueDateThenCreatedAtUtc()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Tasks.AddRange(
            new TaskItem
            {
                Title = "Completed earliest due",
                DueDate = new DateTime(2026, 4, 1),
                IsCompleted = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc)
            },
            new TaskItem
            {
                Title = "Open later due",
                DueDate = new DateTime(2026, 4, 2),
                IsCompleted = false,
                CreatedAtUtc = new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc)
            },
            new TaskItem
            {
                Title = "Open earlier due later created",
                DueDate = new DateTime(2026, 4, 1),
                IsCompleted = false,
                CreatedAtUtc = new DateTime(2026, 1, 3, 8, 0, 0, DateTimeKind.Utc)
            },
            new TaskItem
            {
                Title = "Open earlier due earlier created",
                DueDate = new DateTime(2026, 4, 1),
                IsCompleted = false,
                CreatedAtUtc = new DateTime(2026, 1, 1, 7, 0, 0, DateTimeKind.Utc)
            });

        await dbContext.SaveChangesAsync();

        var service = new TaskService(dbContext);

        var result = await service.GetAllAsync();

        Assert.Equal(
            [
                "Open earlier due earlier created",
                "Open earlier due later created",
                "Open later due",
                "Completed earliest due"
            ],
            result.Select(task => task.Title));
    }

    [Fact]
    public async Task AddAsync_TrimsFieldsSetsDefaultsAndPersists()
    {
        await using var dbContext = CreateDbContext();
        var service = new TaskService(dbContext);
        var before = DateTime.UtcNow;

        var created = await service.AddAsync(new NewTaskInputModel
        {
            Title = "  Write tests  ",
            Description = "  Cover edge cases  ",
            DueDate = new DateTime(2026, 4, 10)
        });

        var after = DateTime.UtcNow;
        var persisted = await dbContext.Tasks.SingleAsync();

        Assert.Equal("Write tests", created.Title);
        Assert.Equal("Cover edge cases", created.Description);
        Assert.Equal(new DateTime(2026, 4, 10), created.DueDate);
        Assert.False(created.IsCompleted);
        Assert.InRange(created.CreatedAtUtc, before, after);
        Assert.Equal(created.Id, persisted.Id);
    }

    [Fact]
    public async Task AddAsync_WhitespaceDescriptionStoresNull()
    {
        await using var dbContext = CreateDbContext();
        var service = new TaskService(dbContext);

        var created = await service.AddAsync(new NewTaskInputModel
        {
            Title = "Task",
            Description = "   ",
            DueDate = new DateTime(2026, 5, 2)
        });

        Assert.Null(created.Description);
        Assert.Null((await dbContext.Tasks.SingleAsync()).Description);
    }

    [Fact]
    public async Task AddAsync_WithNullInput_ThrowsNullReferenceException()
    {
        await using var dbContext = CreateDbContext();
        var service = new TaskService(dbContext);

        await Assert.ThrowsAsync<NullReferenceException>(() => service.AddAsync(null!));
    }

    [Fact]
    public async Task AddAsync_WithNullTitle_ThrowsNullReferenceException()
    {
        await using var dbContext = CreateDbContext();
        var service = new TaskService(dbContext);

        var input = new NewTaskInputModel
        {
            Title = null!,
            DueDate = new DateTime(2026, 5, 2)
        };

        await Assert.ThrowsAsync<NullReferenceException>(() => service.AddAsync(input));
    }

    [Fact]
    public async Task AddAsync_WithNullDueDate_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var service = new TaskService(dbContext);

        var input = new NewTaskInputModel
        {
            Title = "Task",
            DueDate = null
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(input));
    }

    [Fact]
    public async Task MarkCompleteAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var service = new TaskService(dbContext);

        var result = await service.MarkCompleteAsync(404);

        Assert.False(result);
    }

    [Fact]
    public async Task MarkCompleteAsync_ReturnsTrue_AndSetsCompleted_WhenTaskExists()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Tasks.Add(new TaskItem
        {
            Title = "Task",
            DueDate = new DateTime(2026, 4, 20),
            IsCompleted = false,
            CreatedAtUtc = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var taskId = await dbContext.Tasks.Select(item => item.Id).SingleAsync();
        var service = new TaskService(dbContext);

        var result = await service.MarkCompleteAsync(taskId);
        var persisted = await dbContext.Tasks.SingleAsync();

        Assert.True(result);
        Assert.True(persisted.IsCompleted);
    }

    [Fact]
    public async Task MarkCompleteAsync_IsIdempotent_WhenTaskAlreadyCompleted()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Tasks.Add(new TaskItem
        {
            Title = "Task",
            DueDate = new DateTime(2026, 4, 20),
            IsCompleted = true,
            CreatedAtUtc = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var taskId = await dbContext.Tasks.Select(item => item.Id).SingleAsync();
        var service = new TaskService(dbContext);

        var result = await service.MarkCompleteAsync(taskId);
        var persisted = await dbContext.Tasks.SingleAsync();

        Assert.True(result);
        Assert.True(persisted.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var service = new TaskService(dbContext);

        var result = await service.DeleteAsync(404);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_AndRemovesTask_WhenTaskExists()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Tasks.Add(new TaskItem
        {
            Title = "Task",
            DueDate = new DateTime(2026, 4, 20),
            IsCompleted = false,
            CreatedAtUtc = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var taskId = await dbContext.Tasks.Select(item => item.Id).SingleAsync();
        var service = new TaskService(dbContext);

        var result = await service.DeleteAsync(taskId);

        Assert.True(result);
        Assert.Empty(await dbContext.Tasks.ToListAsync());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}