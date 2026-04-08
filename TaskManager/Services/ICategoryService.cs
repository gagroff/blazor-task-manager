using TaskManager.Models;

namespace TaskManager.Services;

public interface ICategoryService
{
    /// <summary>Gets all categories ordered by name.</summary>
    Task<IReadOnlyList<Category>> GetAllAsync();

    /// <summary>Creates a new category from the given input and returns the saved entity.</summary>
    Task<Category> AddAsync(NewCategoryInputModel input);

    /// <summary>Updates the name and color of an existing category. Returns false if not found.</summary>
    Task<bool> UpdateAsync(int id, NewCategoryInputModel input);

    /// <summary>Deletes a category by id. Returns false if not found.</summary>
    Task<bool> DeleteAsync(int id);
}
