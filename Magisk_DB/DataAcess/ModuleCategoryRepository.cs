using Magisk_DB.IDataAcess;
using Magisk_DB.DataAcess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magisk_DB.IDataAcess;
using Magisk_DB.DataAcess;
using Magisk_DB.Services.Classes;
using Magisk_DB.Services.Exceptions;
using Magisk_DB.Services;
using MagiskHub.Models;

namespace Magisk_DB.DataAcess
{
    public class ModuleCategoryRepository : IModuleCategoryRepository
    {
        private readonly IDbContextFactory<MagiskHubContext> _contextFactory;
        public ModuleCategoryRepository(IDbContextFactory<MagiskHubContext> contextFactory) { _contextFactory = contextFactory; }

        public ModuleCategory GetById(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.ModuleCategories.AsNoTracking().FirstOrDefault(c => c.CategoryID == categoryId);
        }
        public ModuleCategory GetByName(string categoryName)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.ModuleCategories.AsNoTracking().FirstOrDefault(c => c.CategoryName.ToLower() == categoryName.ToLower());
        }
        public IEnumerable<ModuleCategory> GetAll()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.ModuleCategories.AsNoTracking().ToList();
        }
        public void Add(ModuleCategory category)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                context.ModuleCategories.Add(category);
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка добавления категории '{category.CategoryName}'.", ex); }
        }
        public void Update(ModuleCategory category)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                context.ModuleCategories.Update(category);
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка обновления категории '{category.CategoryName}'.", ex); }
        }
        public bool Delete(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            var category = context.ModuleCategories.Find(categoryId);
            if (category == null) return false;
            try
            {
                context.ModuleCategories.Remove(category);
                context.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка удаления категории ID {categoryId}. Возможно, она используется.", ex); }
        }
        public bool IsCategoryInUse(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Modules.Any(m => m.CategoryID == categoryId);
        }
    }
}
