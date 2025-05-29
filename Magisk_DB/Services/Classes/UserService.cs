using Magisk_DB.DataAcess;
using Magisk_DB.IDataAcess;
using Magisk_DB.Services.Exceptions;
using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Magisk_DB.Services.Classes
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        private string SimpleHashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        public async Task<User> RegisterAsync(string username, string email, string password, string roleName = "EndUser")
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new BusinessRuleException("Имя пользователя, email и пароль не могут быть пустыми.");

            if (_userRepository.GetUserByUsername(username) != null)
                throw new BusinessRuleException("Пользователь с таким именем уже существует.");

            if (_userRepository.GetUserByEmail(email) != null)
                throw new BusinessRuleException("Пользователь с таким email уже существует.");

            var role = _roleRepository.GetRoleByName(roleName);
            if (role == null)
            {
                // Если критически важная роль "EndUser" не найдена из SeedData, это проблема конфигурации
                var allRoles = _roleRepository.GetAllRoles();
                role = allRoles.FirstOrDefault(r => r.RoleName == "EndUser") ?? allRoles.FirstOrDefault();
                if (role == null) throw new BusinessRuleException("Роли не настроены в системе. Регистрация невозможна.");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = SimpleHashPassword(password),
                RoleID = role.RoleID,
                RegistrationDate = DateTime.UtcNow
            };

            try
            {
                _userRepository.AddUser(user);
                user.Role = role; // Для возврата полной сущности
                return user;
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Ошибка при сохранении данных пользователя.", ex);
            }
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new BusinessRuleException("Имя пользователя и пароль не могут быть пустыми.");

            var user = _userRepository.GetUserByUsernameWithRole(username);

            if (user == null || user.PasswordHash != SimpleHashPassword(password))
                throw new NotFoundException("Неверное имя пользователя или пароль.");

            return user;
        }
    }
}
