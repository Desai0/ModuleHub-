using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magisk_DB.IDataAcess;
using MagiskHub.Models;
using Microsoft.EntityFrameworkCore;

namespace Magisk_DB.DataAcess
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IDbContextFactory<MagiskHubContext> _contextFactory;
        public RoleRepository(IDbContextFactory<MagiskHubContext> contextFactory) { _contextFactory = contextFactory; }

        public Role GetRoleById(int roleId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Roles.AsNoTracking().FirstOrDefault(r => r.RoleID == roleId);
        }
        public Role GetRoleByName(string roleName)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Roles.AsNoTracking().FirstOrDefault(r => r.RoleName == roleName);
        }
        public IEnumerable<Role> GetAllRoles()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Roles.AsNoTracking().ToList();
        }
    }
}
