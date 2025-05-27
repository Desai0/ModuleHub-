using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.IDataAcess
{
    public interface IModuleCategoryRepository
    {
        ModuleCategory GetById(int categoryId);
        ModuleCategory GetByName(string categoryName);
        IEnumerable<ModuleCategory> GetAll();
        void Add(ModuleCategory category);
        void Update(ModuleCategory category);
        bool Delete(int categoryId);
        bool IsCategoryInUse(int categoryId);
    }
}
