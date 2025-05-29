using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.Services
{
    public interface IModerationService
    {
        // Категории
        Task<IEnumerable<ModuleCategory>> GetAllCategoriesAsync();
        Task<ModuleCategory> AddCategoryAsync(string name, string description);
        Task<ModuleCategory> UpdateCategoryAsync(int categoryId, string newName, string newDescription);
        Task DeleteCategoryAsync(int categoryId);

        // Теги
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag> AddTagAsync(string name);
        Task<Tag> UpdateTagAsync(int tagId, string newName);
        Task DeleteTagAsync(int tagId);
        Task AssignTagsToModuleAsync(int moduleId, IEnumerable<int> tagIds); // Новое для управления тегами модуля

        // Верификация модулей
        Task<IEnumerable<Module>> GetUnverifiedModulesAsync();
        Task<Module> VerifyModuleAsync(int moduleId, bool isVerified);
    }
}
