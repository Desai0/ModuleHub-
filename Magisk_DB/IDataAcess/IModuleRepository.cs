using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.IDataAcess
{
    public interface IModuleRepository
    {
        Module GetModuleById(int moduleId);
        Module GetModuleByIdWithDetails(int moduleId); // Включает автора, категорию, версии, теги
        IEnumerable<Module> GetAllModulesWithAuthorAndCategory();
        IEnumerable<Module> SearchModulesByName(string searchTerm);
        IEnumerable<Module> GetModulesByAuthor(int authorUserId);
        IEnumerable<Module> GetUnverifiedModules();
        void AddModuleWithFirstVersion(Module module, ModuleVersion firstVersion); // Транзакционно
        void UpdateModule(Module module); // Для IsVerified, LastUpdateDate
        void AddTagToModule(int moduleId, int tagId); // Для управления тегами модуля
        void RemoveTagFromModule(int moduleId, int tagId);
        Task AddModuleAsync(Module module); // Изменили с AddModuleWithVersionAsync
        Task<Module> GetModuleByNameAsync(string name); // Для проверки уникальности
    }
}
