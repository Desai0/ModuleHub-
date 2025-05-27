using Magisk_DB.IDataAcess;
using MagiskHub.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.DataAcess
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly IDbContextFactory<MagiskHubContext> _contextFactory;
        public ModuleRepository(IDbContextFactory<MagiskHubContext> contextFactory) { _contextFactory = contextFactory; }

        public Module GetModuleById(int moduleId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Modules.Find(moduleId); // Find ищет только по PK, без Include
        }

        public Module GetModuleByIdWithDetails(int moduleId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Modules
                .Include(m => m.Author)
                .Include(m => m.Category)
                .Include(m => m.Versions)
                .Include(m => m.ModuleTags)
                    .ThenInclude(mt => mt.Tag) // Для загрузки самих тегов
                .AsNoTracking()
                .FirstOrDefault(m => m.ModuleID == moduleId);
        }

        public IEnumerable<Module> GetAllModulesWithAuthorAndCategory()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Modules
                .Include(m => m.Author)
                .Include(m => m.Category)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Module> SearchModulesByName(string searchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Modules
                .Include(m => m.Author)
                .Include(m => m.Category)
                .Where(m => m.Name.ToLower().Contains(searchTerm.ToLower()))
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Module> GetModulesByAuthor(int authorUserId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Modules
                .Where(m => m.AuthorUserID == authorUserId)
                .Include(m => m.Category)
                .Include(m => m.Versions)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Module> GetUnverifiedModules()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Modules
                .Where(m => !m.IsVerified)
                .Include(m => m.Author) // Чтобы показать автора
                .AsNoTracking()
                .ToList();
        }

        public void AddModuleWithFirstVersion(Module module, ModuleVersion firstVersion)
        {
            using var context = _contextFactory.CreateDbContext();
            // Убедимся, что firstVersion связана с module
            if (firstVersion.Module == null) firstVersion.Module = module;
            if (module.Versions == null) module.Versions = new List<ModuleVersion>();
            if (!module.Versions.Contains(firstVersion)) module.Versions.Add(firstVersion);

            try
            {
                context.Modules.Add(module); // EF Core также добавит связанные новые ModuleVersion
                context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException($"Ошибка при добавлении модуля '{module.Name}' с первой версией.", ex);
            }
        }

        public void UpdateModule(Module module) // Например, для IsVerified
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                // Если модуль уже отслеживается, изменения будут сохранены.
                // Если нет, его нужно прикрепить и указать, что он изменен.
                var existingModule = context.Modules.Local.FirstOrDefault(e => e.ModuleID == module.ModuleID);
                if (existingModule != null)
                {
                    // Обновляем значения отслеживаемой сущности
                    context.Entry(existingModule).CurrentValues.SetValues(module);
                }
                else
                {
                    // Если сущность не отслеживается, прикрепляем и помечаем как измененную
                    context.Modules.Update(module);
                }
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка обновления модуля '{module.Name}'.", ex); }
        }

        public void AddTagToModule(int moduleId, int tagId)
        {
            using var context = _contextFactory.CreateDbContext();
            var moduleTag = new ModuleTag { ModuleID = moduleId, TagID = tagId };
            try
            {
                // Проверка, что такая связь уже не существует
                if (!context.ModuleTags.Any(mt => mt.ModuleID == moduleId && mt.TagID == tagId))
                {
                    context.ModuleTags.Add(moduleTag);
                    context.SaveChanges();
                }
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка добавления тега ID {tagId} к модулю ID {moduleId}.", ex); }
        }

        public void RemoveTagFromModule(int moduleId, int tagId)
        {
            using var context = _contextFactory.CreateDbContext();
            var moduleTag = context.ModuleTags.FirstOrDefault(mt => mt.ModuleID == moduleId && mt.TagID == tagId);
            if (moduleTag != null)
            {
                try
                {
                    context.ModuleTags.Remove(moduleTag);
                    context.SaveChanges();
                }
                catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка удаления тега ID {tagId} с модуля ID {moduleId}.", ex); }
            }
        }
    }
}
