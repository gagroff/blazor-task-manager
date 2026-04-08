using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskManager.Models;
using TaskManager.Services;
using TasksPage = TaskManager.Components.Pages.Tasks;

namespace TaskManager.Tests;

public class TasksPageTests : TestContext
{
    [Fact]
    public void RendersFormAndEmptyState_WhenNoTasks()
    {
        var serviceMock = new Mock<ITaskService>();
        serviceMock
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync([]);

        Services.AddSingleton(serviceMock.Object);

        var cut = RenderComponent<TasksPage>();

        Assert.NotNull(cut.Find("form"));
        Assert.NotNull(cut.Find("#title"));
        Assert.NotNull(cut.Find("#dueDate"));
        Assert.Contains("No tasks yet. Add your first task above.", cut.Markup);
    }

    [Fact]
    public void SubmittingValidForm_CallsAddAsync_AndRefreshesList()
    {
        var store = new List<TaskItem>();
        var serviceMock = BuildServiceMock(store);
        Services.AddSingleton(serviceMock.Object);

        var cut = RenderComponent<TasksPage>();

        cut.Find("#title").Change("Write tests");
        cut.Find("#description").Change("Cover happy paths");
        cut.Find("#dueDate").Change("2026-04-20");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            serviceMock.Verify(service =>
                service.AddAsync(It.Is<NewTaskInputModel>(input =>
                    input.Title == "Write tests" &&
                    input.Description == "Cover happy paths" &&
                    input.DueDate == new DateTime(2026, 4, 20))),
                Times.Once);
            Assert.Contains("Write tests", cut.Markup);
            Assert.DoesNotContain("No tasks yet. Add your first task above.", cut.Markup);
        });

        serviceMock.Verify(service => service.GetAllAsync(), Times.AtLeast(2));
    }

    [Fact]
    public void ClickingComplete_CallsMarkComplete_AndShowsCompletedState()
    {
        var store = new List<TaskItem>
        {
            new()
            {
                Id = 1,
                Title = "Open item",
                Description = "Still open",
                DueDate = new DateTime(2026, 4, 11),
                IsCompleted = false,
                CreatedAtUtc = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        var serviceMock = BuildServiceMock(store);
        Services.AddSingleton(serviceMock.Object);

        var cut = RenderComponent<TasksPage>();

        cut.Find("button.btn-outline-success").Click();

        cut.WaitForAssertion(() =>
        {
            serviceMock.Verify(service => service.MarkCompleteAsync(1), Times.Once);
            Assert.Contains("Completed", cut.Markup);
            Assert.True(cut.Find("button.btn-outline-success").HasAttribute("disabled"));
        });
    }

    [Fact]
    public void ClickingDelete_CallsDelete_AndRemovesTaskAfterReload()
    {
        var store = new List<TaskItem>
        {
            new()
            {
                Id = 1,
                Title = "Disposable item",
                Description = "To be removed",
                DueDate = new DateTime(2026, 4, 11),
                IsCompleted = false,
                CreatedAtUtc = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        var serviceMock = BuildServiceMock(store);
        Services.AddSingleton(serviceMock.Object);

        var cut = RenderComponent<TasksPage>();

        cut.Find("button.btn-outline-danger").Click();

        cut.WaitForAssertion(() =>
        {
            serviceMock.Verify(service => service.DeleteAsync(1), Times.Once);
            Assert.Contains("No tasks yet. Add your first task above.", cut.Markup);
            Assert.DoesNotContain("Disposable item", cut.Markup);
        });
    }

    [Fact]
    public void SubmittingInvalidForm_ShowsRequiredValidationMessages_AndDoesNotCallAddAsync()
    {
        var store = new List<TaskItem>();
        var serviceMock = BuildServiceMock(store);
        Services.AddSingleton(serviceMock.Object);

        var cut = RenderComponent<TasksPage>();

        cut.Find("#title").Change(string.Empty);
        cut.Find("#dueDate").Change(string.Empty);
        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Title is required.", cut.Markup);
            Assert.Contains("Due date is required.", cut.Markup);
        });

        serviceMock.Verify(service => service.AddAsync(It.IsAny<NewTaskInputModel>()), Times.Never);
    }

    private static Mock<ITaskService> BuildServiceMock(List<TaskItem> store)
    {
        var mock = new Mock<ITaskService>();

        mock
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(() => store
                .OrderBy(task => task.IsCompleted)
                .ThenBy(task => task.DueDate)
                .ThenBy(task => task.CreatedAtUtc)
                .Select(Clone)
                .ToList());

        mock
            .Setup(service => service.AddAsync(It.IsAny<NewTaskInputModel>()))
            .ReturnsAsync((NewTaskInputModel input) =>
            {
                var nextId = store.Count == 0 ? 1 : store.Max(task => task.Id) + 1;
                var item = new TaskItem
                {
                    Id = nextId,
                    Title = input.Title,
                    Description = input.Description,
                    DueDate = input.DueDate!.Value.Date,
                    IsCompleted = false,
                    CreatedAtUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddMinutes(nextId)
                };

                store.Add(item);
                return Clone(item);
            });

        mock
            .Setup(service => service.MarkCompleteAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) =>
            {
                var item = store.FirstOrDefault(task => task.Id == id);
                if (item is null)
                {
                    return false;
                }

                item.IsCompleted = true;
                return true;
            });

        mock
            .Setup(service => service.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) =>
            {
                var item = store.FirstOrDefault(task => task.Id == id);
                if (item is null)
                {
                    return false;
                }

                store.Remove(item);
                return true;
            });

        return mock;
    }

    private static TaskItem Clone(TaskItem source)
    {
        return new TaskItem
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            DueDate = source.DueDate,
            IsCompleted = source.IsCompleted,
            CreatedAtUtc = source.CreatedAtUtc
        };
    }
}