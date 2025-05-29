using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.Services
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string username, string email, string password, string roleName = "EndUser");
        Task<User> LoginAsync(string username, string password);
    }
}
