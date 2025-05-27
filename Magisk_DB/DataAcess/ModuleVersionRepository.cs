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
    public class ModuleVersionRepository : IModuleVersionRepository
    {
        private readonly IDbContextFactory<MagiskHubContext> _contextFactory;
        public ModuleVersionRepository(IDbContextFactory<MagiskHubContext> contextFactory) { _contextFactory = contextFactory; }

        public ModuleVersion GetById(int versionId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.ModuleVersions.AsNoTracking().FirstOrDefault(v => v.VersionID == versionId);
        }
        public void Add(ModuleVersion version)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                context.ModuleVersions.Add(version);
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка добавления версии '{version.VersionString}'.", ex); }
        }
    }
}
