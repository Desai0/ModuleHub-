using Magisk_DB.IDataAcess;
using MagiskHub.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Exception;

namespace Magisk_DB.DataAcess
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContextFactory<MagiskHubContext> _contextFactory;
        public UserRepository(IDbContextFactory<MagiskHubContext> contextFactory) { _contextFactory = contextFactory; }

        public User GetUserById(int userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Users.Include(u => u.Role).AsNoTracking().FirstOrDefault(u => u.UserID == userId);
        }
        public User GetUserByUsername(string username)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Users.AsNoTracking().FirstOrDefault(u => u.Username == username);
        }
        public User GetUserByUsernameWithRole(string username)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Users.Include(u => u.Role).AsNoTracking().FirstOrDefault(u => u.Username == username);
        }
        public User GetUserByEmail(string email)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Users.AsNoTracking().FirstOrDefault(u => u.Email == email);
        }
        public void AddUser(User user)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                context.Users.Add(user);
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException("Ошибка добавления пользователя.", ex); }
        }
    }
}
