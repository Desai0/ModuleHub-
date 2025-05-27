using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.IDataAcess
{
    public interface IUserRepository
    {
        User GetUserById(int userId);
        User GetUserByUsername(string username);
        User GetUserByUsernameWithRole(string username);
        User GetUserByEmail(string email);
        void AddUser(User user);
        // void UpdateUser(User user); // Потенциально
    }
}
