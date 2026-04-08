using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class CategoryService(AppDbContext dbContext) : ICategoryService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Category> AddAsync(NewCategoryInputModel input)
    {
        var category = new Category
        {
            Name = input.Name.Trim(),
            Color = string.IsNullOrWhiteSpace(input.Color) ? null : input.Color.Trim()
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        return category;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, NewCategoryInputModel input)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return false;
        }

        category.Name = input.Name.Trim();
        category.Color = string.IsNullOrWhiteSpace(input.Color) ? null : input.Color.Trim();

        await dbContext.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            return false;
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
