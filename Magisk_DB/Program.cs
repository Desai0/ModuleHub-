using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Text;
using System.Security.Cryptography;
using Magisk_DB.IDataAcess;
using Magisk_DB.DataAcess;
using Microsoft.Extensions.DependencyInjection;
using Magisk_DB.Services.Classes;
using Magisk_DB.Services.Exceptions;
using Magisk_DB.Services;
//using MagiskHub.Data;

namespace MagiskHub.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } // "EndUser", "Developer", "Moderator"

        // Навигационное свойство для связи "один-ко-многим" с Users
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }

    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int RoleID { get; set; }
        [ForeignKey("RoleID")] // Явное указание внешнего ключа
        public virtual Role Role { get; set; } // Навигационное свойство к Role

        // Навигационное свойство для связи "один-к-одному" с DeveloperProfile
        public virtual DeveloperProfile DeveloperProfile { get; set; }

        // Навигационные свойства для связей "один-ко-многим"
        public virtual ICollection<Module> AuthoredModules { get; set; } = new List<Module>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CompatibilityReport> CompatibilityReports { get; set; } = new List<CompatibilityReport>();
    }

    public class DeveloperProfile
    {
        [Key]
        [ForeignKey("User")] // Этот PK 1:1
        public int UserID { get; set; } // Совпадает с UserID из Users

        public string Bio { get; set; }

        [StringLength(255)]
        public string WebsiteUrl { get; set; }

        // Навигационное свойство обратной связи к User
        public virtual User User { get; set; }
    }

    public class ModuleCategory
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }

        public string Description { get; set; }

        // Навигационное свойство для связи "один-ко-многим" с Modules
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
    }

    public class Module
    {
        [Key]
        public int ModuleID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int AuthorUserID { get; set; }
        [ForeignKey("AuthorUserID")]
        public virtual User Author { get; set; }

        // Внешний ключ для Category
        [Required]
        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public virtual ModuleCategory Category { get; set; } // Навигационное свойство к ModuleCategory

        public bool IsVerified { get; set; } = false;
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual ICollection<ModuleVersion> Versions { get; set; } = new List<ModuleVersion>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<ModuleTag> ModuleTags { get; set; } = new List<ModuleTag>();
    }

    public class ModuleVersion
    {
        [Key]
        public int VersionID { get; set; }

        // Внешний ключ для Module
        [Required]
        public int ModuleID { get; set; }
        [ForeignKey("ModuleID")]
        public virtual Module Module { get; set; } // Навигационное свойство к Module

        [Required]
        [StringLength(50)]
        public string VersionString { get; set; }

        public string Changelog { get; set; }

        [Required]
        [StringLength(500)]
        public string DownloadLink { get; set; }

        [StringLength(50)]
        public string MinMagiskVersion { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? FileSizeMB { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // Навигационное свойство
        public virtual ICollection<CompatibilityReport> CompatibilityReports { get; set; } = new List<CompatibilityReport>();
    }

    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        // Внешний ключ для Module
        [Required]
        public int ModuleID { get; set; }
        [ForeignKey("ModuleID")]
        public virtual Module Module { get; set; }

        // Внешний ключ для User
        [Required]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [Required]
        [Range(1, 5)] // Ограничение для рейтинга
        public int Rating { get; set; }

        public string CommentText { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
        public bool IsEdited { get; set; } = false;
    }

    public class CompatibilityReport
    {
        [Key]
        public int ReportID { get; set; }

        // Внешний ключ для ModuleVersion
        [Required]
        public int ModuleVersionID { get; set; }
        [ForeignKey("ModuleVersionID")]
        public virtual ModuleVersion ModuleVersion { get; set; }

        // Внешний ключ для User
        [Required]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceModel { get; set; }

        [Required]
        [StringLength(50)]
        public string AndroidVersion { get; set; }

        [Required]
        [StringLength(50)] // "Works", "WorksWithIssues", "DoesNotWork"
        public string WorksStatus { get; set; }

        public string UserNotes { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    }

    public class Tag
    {
        [Key]
        public int TagID { get; set; }

        [Required]
        [StringLength(50)]
        public string TagName { get; set; }

        // Навигационное свойство для связи M:M
        public virtual ICollection<ModuleTag> ModuleTags { get; set; } = new List<ModuleTag>();
    }

    public class ModuleTag
    {
        // Composite Primary Key (сконфигурируем в DbContext)
        public int ModuleID { get; set; }
        public virtual Module Module { get; set; }

        public int TagID { get; set; }
        public virtual Tag Tag { get; set; }
    }
}




public class MagiskHubContext : DbContext
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<DeveloperProfile> DeveloperProfiles { get; set; }
    public DbSet<ModuleCategory> ModuleCategories { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<ModuleVersion> ModuleVersions { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<CompatibilityReport> CompatibilityReports { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ModuleTag> ModuleTags { get; set; } // Join table

    public MagiskHubContext(DbContextOptions<MagiskHubContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Конфигурация связей и ограничений ---

        // Role: Unique RoleName
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.RoleName)
            .IsUnique();

        // User: Unique Username, Unique Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // User <-> Role (Many-to-One / One-to-Many)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleID);

        // User <-> DeveloperProfile (One-to-One)
        modelBuilder.Entity<User>()
            .HasOne(u => u.DeveloperProfile)
            .WithOne(dp => dp.User)
            .HasForeignKey<DeveloperProfile>(dp => dp.UserID); // FK в DeveloperProfile

        // ModuleCategory: Unique CategoryName
        modelBuilder.Entity<ModuleCategory>()
            .HasIndex(mc => mc.CategoryName)
            .IsUnique();

        // Module <-> User (Author) (Many-to-One / One-to-Many)
        modelBuilder.Entity<Module>()
            .HasOne(m => m.Author)
            .WithMany(u => u.AuthoredModules)
            .HasForeignKey(m => m.AuthorUserID)
            .OnDelete(DeleteBehavior.Restrict); // Запретить удаление пользователя, если у него есть модули

        // Module <-> ModuleCategory (Many-to-One / One-to-Many)
        modelBuilder.Entity<Module>()
            .HasOne(m => m.Category)
            .WithMany(mc => mc.Modules)
            .HasForeignKey(m => m.CategoryID);

        // Module <-> ModuleVersion (One-to-Many)
        modelBuilder.Entity<ModuleVersion>()
            .HasOne(mv => mv.Module)
            .WithMany(m => m.Versions)
            .HasForeignKey(mv => mv.ModuleID);

        // Review <-> Module (Many-to-One / One-to-Many)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Module)
            .WithMany(m => m.Reviews)
            .HasForeignKey(r => r.ModuleID);

        // Review <-> User (Many-to-One / One-to-Many)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserID)
            .OnDelete(DeleteBehavior.ClientCascade); // или Restrict, если не хотите удалять отзывы при удалении пользователя

        // CompatibilityReport <-> ModuleVersion (Many-to-One / One-to-Many)
        modelBuilder.Entity<CompatibilityReport>()
            .HasOne(cr => cr.ModuleVersion)
            .WithMany(mv => mv.CompatibilityReports)
            .HasForeignKey(cr => cr.ModuleVersionID);

        // CompatibilityReport <-> User (Many-to-One / One-to-Many)
        modelBuilder.Entity<CompatibilityReport>()
           .HasOne(cr => cr.User)
           .WithMany(u => u.CompatibilityReports)
           .HasForeignKey(cr => cr.UserID)
           .OnDelete(DeleteBehavior.ClientCascade); // или Restrict

        // Tag: Unique TagName
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.TagName)
            .IsUnique();

        // Many-to-Many: Module <-> Tag через ModuleTag
        modelBuilder.Entity<ModuleTag>()
            .HasKey(mt => new { mt.ModuleID, mt.TagID }); // Composite Primary Key

        modelBuilder.Entity<ModuleTag>()
            .HasOne(mt => mt.Module)
            .WithMany(m => m.ModuleTags)
            .HasForeignKey(mt => mt.ModuleID);

        modelBuilder.Entity<ModuleTag>()
            .HasOne(mt => mt.Tag)
            .WithMany(t => t.ModuleTags)
            .HasForeignKey(mt => mt.TagID);

        // Заполнение начальными данными (Seed Data) - опционально
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleID = 1, RoleName = "EndUser" },
            new Role { RoleID = 2, RoleName = "Developer" },
            new Role { RoleID = 3, RoleName = "Moderator" }
        );
    }
}

class Program
{
    private static IUserService _userService;
    private static IModuleService _moduleService;
    private static IModerationService _moderationService;
    private static User _currentUser = null;

    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=Magisk;Username=postgres;Password=1234";

        services.AddDbContextFactory<MagiskHubContext>(options => options.UseNpgsql(connectionString));
        services.AddDbContext<MagiskHubContext>(options => options.UseNpgsql(connectionString), ServiceLifetime.Scoped);

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IModuleCategoryRepository, ModuleCategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IModuleVersionRepository, ModuleVersionRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ICompatibilityReportRepository, CompatibilityReportRepository>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<IModerationService, ModerationService>();

        var serviceProvider = services.BuildServiceProvider();

        _userService = serviceProvider.GetRequiredService<IUserService>();
        _moduleService = serviceProvider.GetRequiredService<IModuleService>();
        _moderationService = serviceProvider.GetRequiredService<IModerationService>();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MagiskHubContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        Console.WriteLine("Добро пожаловать в MagiskHub Console!");
        Console.WriteLine("-----------------------------------");

        while (true)
        {
            if (_currentUser == null)
            {
                await ShowMainMenuUnauthenticatedAsync();
            }
            else
            {
                await ShowMainMenuAuthenticatedAsync();
            }
        }
    }

    static string ReadNonEmptyLine(string prompt, bool allowEmptyForUpdate = false)
    {
        string input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();
            if (allowEmptyForUpdate && string.IsNullOrEmpty(input)) return input; // Разрешаем пустой ввод для обновлений

            if (string.IsNullOrWhiteSpace(input) && !allowEmptyForUpdate)
            {
                Console.WriteLine("Ввод не может быть пустым. Пожалуйста, попробуйте снова.");
            }
        } while (string.IsNullOrWhiteSpace(input) && !allowEmptyForUpdate);
        return input;
    }

    static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue, bool allowEmptyForUpdate = false, int defaultValueIfEmpty = -1)
    {
        int value;
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();

            if (allowEmptyForUpdate && string.IsNullOrEmpty(input)) return defaultValueIfEmpty;

            if (int.TryParse(input, out value) && value >= min && value <= max)
            {
                return value;
            }
            Console.WriteLine($"Некорректный ввод. Пожалуйста, введите целое число от {min} до {max}.");
        }
    }

    static async Task ExecuteServiceLogicAsync(Func<Task> serviceAction, string successMessage = "Операция выполнена успешно.")
    {
        try
        {
            await serviceAction();
            if (!string.IsNullOrEmpty(successMessage))
            {
                Console.WriteLine(successMessage);
            }
        }
        catch (NotFoundException ex) { Console.WriteLine($"Не найдено: {ex.Message}"); }
        catch (BusinessRuleException ex) { Console.WriteLine($"Ошибка бизнес-логики: {ex.Message}"); }
        catch (DataAccessException ex) { Console.WriteLine($"Ошибка доступа к данным: {ex.Message} (Подробнее: {ex.InnerException?.Message})"); }
        catch (Exception ex) { Console.WriteLine($"Произошла непредвиденная ошибка: {ex.Message}"); }
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
        Console.Clear(); // Очистка экрана после операции
    }

    // --- МЕНЮ ---
    // ShowMainMenuUnauthenticatedAsync и ShowMainMenuAuthenticatedAsync остаются такими же, как в предыдущем ответе
    static async Task ShowMainMenuUnauthenticatedAsync()
    {
        Console.WriteLine("\nМеню Гостя:");
        Console.WriteLine("1. Просмотр всех модулей");
        Console.WriteLine("2. Поиск модуля по названию");
        Console.WriteLine("3. Регистрация");
        Console.WriteLine("4. Вход");
        Console.WriteLine("0. Выход из приложения");
        Console.Write("Ваш выбор: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1": await ViewAllModulesUIAsync(); break;
            case "2": await SearchModuleByNameUIAsync(); break;
            case "3": await RegisterUserUIAsync(); break;
            case "4": await LoginUserUIAsync(); break;
            case "0": Environment.Exit(0); break;
            default: Console.WriteLine("Неверный выбор. Попробуйте снова."); break;
        }
    }

    static async Task ShowMainMenuAuthenticatedAsync()
    {
        Console.WriteLine($"\nМеню (Пользователь: {_currentUser.Username} | Роль: {_currentUser.Role.RoleName}):");
        Console.WriteLine("1. Просмотр всех модулей");
        Console.WriteLine("2. Поиск модуля по названию");
        Console.WriteLine("3. Просмотр деталей модуля");
        Console.WriteLine("4. Оставить отзыв на модуль");
        Console.WriteLine("5. Сообщить о совместимости версии модуля");

        if (_currentUser.Role.RoleName == "Developer")
        {
            Console.WriteLine("--- Меню Разработчика ---");
            Console.WriteLine("D1. Загрузить новый модуль");
            Console.WriteLine("D2. Добавить новую версию к моему модулю");
            Console.WriteLine("D3. Просмотреть мои модули");
            Console.WriteLine("D4. Управление тегами моего модуля");
        }
        if (_currentUser.Role.RoleName == "Moderator")
        {
            Console.WriteLine("--- Меню Модератора ---");
            Console.WriteLine("M1. Верифицировать модуль");
            Console.WriteLine("M2. Управление категориями");
            Console.WriteLine("M3. Управление тегами (глобально)");
        }

        Console.WriteLine("9. Выйти из аккаунта");
        Console.WriteLine("0. Выход из приложения");
        Console.Write("Ваш выбор: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1": await ViewAllModulesUIAsync(); break;
            case "2": await SearchModuleByNameUIAsync(); break;
            case "3": await ViewModuleDetailsUIAsync(); break;
            case "4": await AddReviewUIAsync(); break;
            case "5": await ReportCompatibilityUIAsync(); break;
            case "D1": if (_currentUser.Role.RoleName == "Developer") await UploadNewModuleUIAsync(); else Console.WriteLine("Доступ запрещен."); break;
            case "D2": if (_currentUser.Role.RoleName == "Developer") await AddVersionToMyModuleUIAsync(); else Console.WriteLine("Доступ запрещен."); break;
            case "D3": if (_currentUser.Role.RoleName == "Developer") await ViewMyModulesUIAsync(); else Console.WriteLine("Доступ запрещен."); break;
            case "D4": if (_currentUser.Role.RoleName == "Developer") await ManageMyModuleTagsUIAsync(); else Console.WriteLine("Доступ запрещен."); break;
            case "M1": if (_currentUser.Role.RoleName == "Moderator") await VerifyModuleUIAsync(); else Console.WriteLine("Доступ запрещен."); break;
            case "M2": if (_currentUser.Role.RoleName == "Moderator") await ManageCategoriesMenuUIAsync(); else Console.WriteLine("Доступ запрещен."); break;
            case "M3": if (_currentUser.Role.RoleName == "Moderator") await ManageGlobalTagsMenuUIAsync(); else Console.WriteLine("Доступ запрещен."); break;
            case "9": LogoutUserUI(); break;
            case "0": Environment.Exit(0); break;
            default: Console.WriteLine("Неверный выбор. Попробуйте снова."); break;
        }
    }

    // --- РЕАЛИЗАЦИЯ UI МЕТОДОВ ---

    // Методы RegisterUserUIAsync, LoginUserUI, LogoutUserUI, ViewAllModulesUIAsync, SearchModuleByNameUIAsync, ViewModuleDetailsUIAsync, UploadNewModuleUIAsync
    // остаются такими же, как в предыдущем ответе.

    // Копирую их для полноты:
    static async Task RegisterUserUIAsync()
    {
        Console.WriteLine("\n--- Регистрация ---");
        string username = ReadNonEmptyLine("Имя пользователя: ");
        string email = ReadNonEmptyLine("Email: ");
        string password = ReadNonEmptyLine("Пароль: ");
        await ExecuteServiceLogicAsync(
            async () => {
                var user = await _userService.RegisterAsync(username, email, password); // Роль по умолчанию "EndUser"
                Console.WriteLine($"Пользователь '{user.Username}' успешно зарегистрирован с ролью '{user.Role.RoleName}'.");
            },
            null
        );
    }

    static async Task LoginUserUIAsync()
    {
        Console.WriteLine("\n--- Вход ---");
        string username = ReadNonEmptyLine("Имя пользователя: ");
        string password = ReadNonEmptyLine("Пароль: ");
        await ExecuteServiceLogicAsync(
            async () => {
                _currentUser = await _userService.LoginAsync(username, password);
                Console.WriteLine($"Добро пожаловать, {_currentUser.Username}!");
            },
            null
        );
    }

    static void LogoutUserUI()
    {
        if (_currentUser != null)
        {
            Console.WriteLine($"Пользователь {_currentUser.Username} вышел из системы.");
            _currentUser = null;
        }
        else
        {
            Console.WriteLine("Вы не вошли в систему.");
        }
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
        Console.Clear();
    }

    static async Task ViewAllModulesUIAsync()
    {
        Console.WriteLine("\n--- Список всех модулей ---");
        await ExecuteServiceLogicAsync(
            async () => {
                var modules = await _moduleService.GetAllModulesAsync();
                if (!modules.Any()) { Console.WriteLine("Модулей пока нет."); return; }
                foreach (var module in modules)
                {
                    Console.WriteLine($"ID: {module.ModuleID}, Название: {module.Name}, Автор: {module.Author?.Username ?? "N/A"}, " +
                                      $"Категория: {module.Category?.CategoryName ?? "N/A"}, Верифицирован: {module.IsVerified}");
                }
            },
            null
        );
    }

    static async Task SearchModuleByNameUIAsync()
    {
        Console.WriteLine("\n--- Поиск модуля ---");
        string searchTerm = ReadNonEmptyLine("Введите название или часть названия для поиска: ");
        await ExecuteServiceLogicAsync(
           async () => {
               var modules = await _moduleService.SearchModulesByNameAsync(searchTerm);
               if (!modules.Any()) { Console.WriteLine($"Модули по запросу '{searchTerm}' не найдены."); return; }
               Console.WriteLine($"\nРезультаты поиска по '{searchTerm}':");
               foreach (var module in modules)
               {
                   Console.WriteLine($"ID: {module.ModuleID}, Название: {module.Name}, Автор: {module.Author?.Username ?? "N/A"}, Категория: {module.Category?.CategoryName ?? "N/A"}");
               }
           },
           null
       );
    }

    static async Task ViewModuleDetailsUIAsync()
    {
        Console.WriteLine("\n--- Детали модуля ---");
        int moduleId = ReadInt("Введите ID модуля для просмотра деталей: ", 1);
        await ExecuteServiceLogicAsync(
            async () => {
                var module = await _moduleService.GetModuleByIdAsync(moduleId);
                Console.WriteLine($"\nДетали модуля: {module.Name} (ID: {module.ModuleID})");
                Console.WriteLine($"Автор: {module.Author.Username}");
                Console.WriteLine($"Категория: {module.Category.CategoryName}");
                Console.WriteLine($"Описание: {module.Description}");
                Console.WriteLine($"Верифицирован: {module.IsVerified}");
                Console.WriteLine($"Создан: {module.CreationDate.ToLocalTime()}, Обновлен: {module.LastUpdateDate.ToLocalTime()}");

                Console.WriteLine("\nВерсии:");
                if (module.Versions != null && module.Versions.Any())
                {
                    foreach (var ver in module.Versions.OrderByDescending(v => v.UploadDate))
                    {
                        Console.WriteLine($"  - ID Версии: {ver.VersionID}, Строка версии: {ver.VersionString}, Загружен: {ver.UploadDate.ToLocalTime().ToShortDateString()}");
                        Console.WriteLine($"    Ссылка: {ver.DownloadLink}");
                        if (!string.IsNullOrWhiteSpace(ver.Changelog)) Console.WriteLine($"    Изменения: {ver.Changelog}");
                    }
                }
                else { Console.WriteLine("  Версий нет."); }

                Console.WriteLine("\nОтзывы:");
                if (module.Reviews != null && module.Reviews.Any())
                {
                    foreach (var rev in module.Reviews.OrderByDescending(r => r.ReviewDate))
                    {
                        Console.WriteLine($"  - От {rev.User?.Username ?? "Аноним"} ({rev.ReviewDate.ToLocalTime().ToShortDateString()}), Оценка: {rev.Rating}/5");
                        Console.WriteLine($"    {rev.CommentText}");
                    }
                }
                else { Console.WriteLine("  Отзывов нет."); }

                Console.WriteLine("\nТеги:");
                if (module.ModuleTags != null && module.ModuleTags.Any())
                {
                    Console.WriteLine($"  {string.Join(", ", module.ModuleTags.Select(mt => mt.Tag?.TagName ?? "N/A"))}");
                }
                else { Console.WriteLine("  Тегов нет."); }
            },
            null
        );
    }

    static async Task UploadNewModuleUIAsync()
    {
        Console.WriteLine("\n--- Загрузка нового модуля ---");
        string name = ReadNonEmptyLine("Название модуля: ");
        string description = ReadNonEmptyLine("Описание: ");

        var categories = (await _moderationService.GetAllCategoriesAsync()).ToList();
        if (!categories.Any()) { Console.WriteLine("Нет доступных категорий. Обратитесь к модератору."); return; }
        Console.WriteLine("Доступные категории:");
        categories.ForEach(cat => Console.WriteLine($"ID: {cat.CategoryID} - {cat.CategoryName}"));
        int categoryId = ReadInt("ID категории: ", 1, categories.Any() ? categories.Max(c => c.CategoryID) : 1);


        string versionString = ReadNonEmptyLine("Начальная версия (например, v1.0): ");
        string downloadLink = ReadNonEmptyLine("Ссылка на скачивание: ");
        Console.Write("Changelog (необязательно): ");
        string changelog = Console.ReadLine();
        string minMagiskVersion = ReadNonEmptyLine("Минимальная требуемая версия Magisk (например, 20.4): "); // Новый ввод

        await ExecuteServiceLogicAsync(
            async () => {
                var module = await _moduleService.UploadNewModuleAsync(
                    _currentUser.UserID,
                    name,
                    description,
                    categoryId,
                    versionString,
                    downloadLink,
                    changelog,
                    minMagiskVersion // Передаем новое значение
                );
                Console.WriteLine($"Модуль '{module.Name}' (ID: {module.ModuleID}) и его версия '{module.Versions.First().VersionString}' успешно загружены и ожидают верификации.");
            },
            null
        );
    }


    static async Task AddReviewUIAsync()
    {
        Console.WriteLine("\n--- Оставить отзыв ---");
        int moduleId = ReadInt("Введите ID модуля, на который хотите оставить отзыв: ", 1);
        int rating = ReadInt("Ваша оценка (1-5): ", 1, 5);
        string comment = ReadNonEmptyLine("Комментарий: ");

        await ExecuteServiceLogicAsync(
            async () => {
                await _moduleService.AddReviewAsync(moduleId, _currentUser.UserID, rating, comment);
            },
            "Отзыв успешно добавлен!"
        );
    }

    static async Task ReportCompatibilityUIAsync()
    {
        Console.WriteLine("\n--- Сообщить о совместимости ---");
        // Сначала нужно дать пользователю выбрать модуль и версию
        await ViewAllModulesUIAsync(); // Показываем все модули
        int moduleIdForReport = ReadInt("Введите ID модуля, для которого хотите сообщить о совместимости: ", 1);
        Module selectedModule = null;
        try { selectedModule = await _moduleService.GetModuleByIdAsync(moduleIdForReport); } catch { /* обработано ExecuteServiceLogicAsync */ }
        if (selectedModule == null) { Console.WriteLine("Модуль не найден или ошибка при получении."); return; }

        if (selectedModule.Versions == null || !selectedModule.Versions.Any())
        {
            Console.WriteLine("У этого модуля нет версий."); return;
        }
        Console.WriteLine("Доступные версии этого модуля:");
        selectedModule.Versions.OrderByDescending(v => v.UploadDate).ToList().ForEach(v => Console.WriteLine($"ID Версии: {v.VersionID} - {v.VersionString}"));
        int moduleVersionId = ReadInt("Введите ID версии модуля: ", 1);


        string deviceModel = ReadNonEmptyLine("Модель вашего устройства: ");
        string androidVersion = ReadNonEmptyLine("Версия Android: ");
        Console.WriteLine("Статус работы: 1-Работает, 2-Работает с проблемами, 3-Не работает");
        string worksStatusInput = ReadNonEmptyLine("Выберите статус (1/2/3): ");
        string worksStatus = worksStatusInput switch
        {
            "1" => "Works",
            "2" => "WorksWithIssues",
            "3" => "DoesNotWork",
            _ => "Unknown" // или выбросить ошибку
        };
        if (worksStatus == "Unknown") { Console.WriteLine("Некорректный статус."); return; }

        Console.Write("Дополнительные заметки (необязательно): ");
        string notes = Console.ReadLine();

        await ExecuteServiceLogicAsync(
            async () => {
                await _moduleService.AddCompatibilityReportAsync(moduleVersionId, _currentUser.UserID, deviceModel, androidVersion, worksStatus, notes);
            },
            "Отчет о совместимости успешно добавлен!"
        );
    }

    static async Task AddVersionToMyModuleUIAsync()
    {
        Console.WriteLine("\n--- Добавить новую версию к модулю ---");
        var myModules = (await _moduleService.GetModulesByAuthorAsync(_currentUser.UserID)).ToList();
        if (!myModules.Any())
        {
            Console.WriteLine("У вас нет загруженных модулей.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
            return;
        }

        Console.WriteLine("Ваши модули:");
        myModules.ForEach(m => Console.WriteLine($"ID: {m.ModuleID} - {m.Name}"));
        int moduleId = ReadInt("Введите ID модуля, к которому хотите добавить версию: ", 1);

        // Проверка, что модуль принадлежит текущему пользователю
        if (!myModules.Any(m => m.ModuleID == moduleId))
        {
            Console.WriteLine("Это не ваш модуль или ID некорректен.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
            return;
        }

        string versionString = ReadNonEmptyLine("Новая версия (например, v1.1): ");
        string downloadLink = ReadNonEmptyLine("Ссылка на скачивание: ");
        Console.Write("Changelog: ");
        string changelog = Console.ReadLine();
        string minMagiskVersion = ReadNonEmptyLine("Минимальная требуемая версия Magisk (например, 20.4): "); // Новый ввод

        await ExecuteServiceLogicAsync(
            async () => {
                var newVersion = await _moduleService.AddVersionToModuleAsync(
                    moduleId,
                    _currentUser.UserID,
                    versionString,
                    downloadLink,
                    changelog,
                    minMagiskVersion // Передаем новое значение
                );
                Console.WriteLine($"Новая версия '{newVersion.VersionString}' для модуля ID {moduleId} успешно добавлена.");
            },
            null // Сообщение об успехе уже внутри лямбды
        );
    }

    static async Task ViewMyModulesUIAsync()
    {
        Console.WriteLine("\n--- Ваши модули ---");
        await ExecuteServiceLogicAsync(
            async () => {
                var myModules = await _moduleService.GetModulesByAuthorAsync(_currentUser.UserID);
                if (!myModules.Any()) { Console.WriteLine("У вас нет загруженных модулей."); return; }
                foreach (var module in myModules)
                {
                    Console.WriteLine($"ID: {module.ModuleID}, Название: {module.Name}, Категория: {module.Category?.CategoryName ?? "N/A"}, Верифицирован: {module.IsVerified}");
                    if (module.Versions != null && module.Versions.Any())
                    {
                        Console.WriteLine("  Версии:");
                        foreach (var ver in module.Versions.OrderByDescending(v => v.UploadDate))
                        {
                            Console.WriteLine($"    - ID: {ver.VersionID}, {ver.VersionString} (Загружено: {ver.UploadDate.ToLocalTime().ToShortDateString()})");
                        }
                    }
                    else { Console.WriteLine("  (Версий нет)"); }
                    Console.WriteLine("  Теги: " + (module.ModuleTags != null && module.ModuleTags.Any() ? string.Join(", ", module.ModuleTags.Select(mt => mt.Tag?.TagName ?? "N/A")) : "Нет тегов"));
                    Console.WriteLine("---");
                }
            },
            null
        );
    }

    static async Task ManageMyModuleTagsUIAsync()
    {
        Console.WriteLine("\n--- Управление тегами моего модуля ---");
        var myModules = (await _moduleService.GetModulesByAuthorAsync(_currentUser.UserID)).ToList();
        if (!myModules.Any()) { Console.WriteLine("У вас нет загруженных модулей."); return; }

        Console.WriteLine("Ваши модули:");
        myModules.ForEach(m => Console.WriteLine($"ID: {m.ModuleID} - {m.Name}"));
        int moduleId = ReadInt("Введите ID модуля для управления тегами: ", 1);

        Module selectedModule = null;
        try { selectedModule = await _moduleService.GetModuleByIdAsync(moduleId); } catch { }
        if (selectedModule == null || selectedModule.AuthorUserID != _currentUser.UserID)
        { Console.WriteLine("Модуль не найден или не принадлежит вам."); return; }

        Console.WriteLine($"Текущие теги для модуля '{selectedModule.Name}': " +
            (selectedModule.ModuleTags.Any() ? string.Join(", ", selectedModule.ModuleTags.Select(mt => mt.Tag.TagName)) : "Нет"));

        var allTags = (await _moderationService.GetAllTagsAsync()).ToList();
        if (!allTags.Any()) { Console.WriteLine("В системе нет тегов для присвоения."); return; }
        Console.WriteLine("\nДоступные теги в системе:");
        allTags.ForEach(t => Console.WriteLine($"ID: {t.TagID} - {t.TagName}"));

        Console.WriteLine("Введите ID тегов, которые должны быть у модуля, через запятую (например, 1,3,5).");
        Console.WriteLine("Чтобы удалить все теги, оставьте поле пустым и нажмите Enter.");
        string tagIdsInput = Console.ReadLine();

        List<int> newTagIds = new List<int>();
        if (!string.IsNullOrWhiteSpace(tagIdsInput))
        {
            try
            {
                newTagIds = tagIdsInput.Split(',').Select(idStr => int.Parse(idStr.Trim())).Distinct().ToList();
            }
            catch { Console.WriteLine("Некорректный формат ввода ID тегов."); return; }
        }

        await ExecuteServiceLogicAsync(
            async () => {
                await _moderationService.AssignTagsToModuleAsync(moduleId, newTagIds);
            },
            $"Теги для модуля '{selectedModule.Name}' обновлены."
        );
    }

    static async Task VerifyModuleUIAsync()
    {
        Console.WriteLine("\n--- Верификация модуля ---");
        var unverifiedModules = (await _moderationService.GetUnverifiedModulesAsync()).ToList();
        if (!unverifiedModules.Any()) { Console.WriteLine("Нет модулей, ожидающих верификации."); return; }

        Console.WriteLine("Модули для верификации:");
        unverifiedModules.ForEach(m => Console.WriteLine($"ID: {m.ModuleID} - {m.Name} (Автор: {m.Author?.Username ?? "N/A"})"));
        int moduleId = ReadInt("Введите ID модуля для изменения статуса верификации: ", 1);

        Module moduleToVerify = null;
        try { moduleToVerify = await _moduleService.GetModuleByIdAsync(moduleId); } catch { } // Получаем модуль, чтобы знать текущий статус
        if (moduleToVerify == null) { Console.WriteLine("Модуль не найден."); return; }


        Console.WriteLine($"Текущий статус верификации для '{moduleToVerify.Name}': {moduleToVerify.IsVerified}");
        Console.Write($"Изменить статус на {!moduleToVerify.IsVerified}? (yes/no): ");
        string choice = ReadNonEmptyLine("").ToLower();

        if (choice == "yes")
        {
            await ExecuteServiceLogicAsync(
                async () => {
                    var updatedModule = await _moderationService.VerifyModuleAsync(moduleId, !moduleToVerify.IsVerified);
                    Console.WriteLine($"Статус верификации модуля '{updatedModule.Name}' изменен на {updatedModule.IsVerified}.");
                },
                null
            );
        }
        else { Console.WriteLine("Операция отменена."); }
    }

    static async Task ManageCategoriesMenuUIAsync()
    {
        while (true)
        {
            Console.WriteLine("\n--- Управление категориями ---");
            Console.WriteLine("1. Показать все категории");
            Console.WriteLine("2. Добавить новую категорию");
            Console.WriteLine("3. Редактировать категорию");
            Console.WriteLine("4. Удалить категорию");
            Console.WriteLine("0. Назад в главное меню");
            Console.Write("Ваш выбор: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": await ListAllCategoriesUIAsync(); break;
                case "2": await AddNewCategoryUIAsync(); break;
                case "3": await EditCategoryUIAsync(); break;
                case "4": await DeleteCategoryUIAsync(); break;
                case "0": Console.Clear(); return;
                default: Console.WriteLine("Неверный выбор."); break;
            }
        }
    }

    static async Task ListAllCategoriesUIAsync()
    {
        Console.WriteLine("\n--- Список категорий ---");
        await ExecuteServiceLogicAsync(
           async () => {
               var categories = await _moderationService.GetAllCategoriesAsync();
               if (!categories.Any()) { Console.WriteLine("Категорий нет."); return; }
               foreach (var cat in categories) { Console.WriteLine($"ID: {cat.CategoryID}, Название: {cat.CategoryName}, Описание: {cat.Description ?? "N/A"}"); }
           },
           null
       );
    }
    static async Task AddNewCategoryUIAsync()
    {
        Console.WriteLine("\n--- Добавление категории ---");
        string name = ReadNonEmptyLine("Название: ");
        Console.Write("Описание (необязательно): ");
        string description = Console.ReadLine();
        await ExecuteServiceLogicAsync(
            async () => {
                var cat = await _moderationService.AddCategoryAsync(name, description);
                Console.WriteLine($"Категория '{cat.CategoryName}' (ID: {cat.CategoryID}) добавлена.");
            }, null
        );
    }
    static async Task EditCategoryUIAsync()
    {
        Console.WriteLine("\n--- Редактирование категории ---");
        await ListAllCategoriesUIAsync(); // Показать для выбора
        int categoryId = ReadInt("Введите ID категории для редактирования: ", 1);
        string newName = ReadNonEmptyLine("Новое название (оставьте пустым, чтобы не менять): ", true);
        Console.Write("Новое описание (оставьте пустым, чтобы не менять, или введите 'clear' для удаления): ");
        string newDescriptionInput = Console.ReadLine();
        string newDescription = newDescriptionInput.ToLower() == "clear" ? "" : newDescriptionInput;


        await ExecuteServiceLogicAsync(
            async () => {
                var cat = await _moderationService.UpdateCategoryAsync(categoryId, newName, newDescription);
                Console.WriteLine($"Категория '{cat.CategoryName}' (ID: {cat.CategoryID}) обновлена.");
            }, null
        );
    }
    static async Task DeleteCategoryUIAsync()
    {
        Console.WriteLine("\n--- Удаление категории ---");
        await ListAllCategoriesUIAsync();
        int categoryId = ReadInt("Введите ID категории для удаления: ", 1);
        Console.Write($"Вы уверены, что хотите удалить категорию ID {categoryId}? Это действие необратимо. (yes/no): ");
        if (ReadNonEmptyLine("").ToLower() == "yes")
        {
            await ExecuteServiceLogicAsync(
                async () => {
                    await _moderationService.DeleteCategoryAsync(categoryId);
                },
                $"Категория ID {categoryId} удалена (если она не использовалась)."
            );
        }
        else { Console.WriteLine("Удаление отменено."); }
    }

    static async Task ManageGlobalTagsMenuUIAsync()
    {
        while (true)
        {
            Console.WriteLine("\n--- Управление тегами (Глобально) ---");
            Console.WriteLine("1. Показать все теги");
            Console.WriteLine("2. Добавить новый тег");
            Console.WriteLine("3. Редактировать тег");
            Console.WriteLine("4. Удалить тег");
            Console.WriteLine("0. Назад в главное меню");
            Console.Write("Ваш выбор: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": await ListAllGlobalTagsUIAsync(); break;
                case "2": await AddNewGlobalTagUIAsync(); break;
                case "3": await EditGlobalTagUIAsync(); break;
                case "4": await DeleteGlobalTagUIAsync(); break;
                case "0": Console.Clear(); return;
                default: Console.WriteLine("Неверный выбор."); break;
            }
        }
    }
    static async Task ListAllGlobalTagsUIAsync()
    {
        Console.WriteLine("\n--- Список всех тегов ---");
        await ExecuteServiceLogicAsync(
            async () => {
                var tags = await _moderationService.GetAllTagsAsync();
                if (!tags.Any()) { Console.WriteLine("Тегов нет."); return; }
                tags.ToList().ForEach(t => Console.WriteLine($"ID: {t.TagID} - {t.TagName}"));
            }, null
        );
    }
    static async Task AddNewGlobalTagUIAsync()
    {
        Console.WriteLine("\n--- Добавление нового тега ---");
        string name = ReadNonEmptyLine("Название тега: ");
        await ExecuteServiceLogicAsync(
            async () => {
                var tag = await _moderationService.AddTagAsync(name);
                Console.WriteLine($"Тег '{tag.TagName}' (ID: {tag.TagID}) добавлен.");
            }, null
        );
    }
    static async Task EditGlobalTagUIAsync()
    {
        Console.WriteLine("\n--- Редактирование тега ---");
        await ListAllGlobalTagsUIAsync();
        int tagId = ReadInt("Введите ID тега для редактирования: ", 1);
        string newName = ReadNonEmptyLine("Новое название тега: ");
        await ExecuteServiceLogicAsync(
           async () => {
               var tag = await _moderationService.UpdateTagAsync(tagId, newName);
               Console.WriteLine($"Тег ID {tag.TagID} обновлен на '{tag.TagName}'.");
           }, null
       );
    }
    static async Task DeleteGlobalTagUIAsync()
    {
        Console.WriteLine("\n--- Удаление тега ---");
        await ListAllGlobalTagsUIAsync();
        int tagId = ReadInt("Введите ID тега для удаления: ", 1);
        Console.Write($"Вы уверены, что хотите удалить тег ID {tagId}? (yes/no): ");
        if (ReadNonEmptyLine("").ToLower() == "yes")
        {
            await ExecuteServiceLogicAsync(
                async () => {
                    await _moderationService.DeleteTagAsync(tagId);
                },
                $"Тег ID {tagId} удален (если он не использовался)."
            );
        }
        else { Console.WriteLine("Удаление отменено."); }
    }

}

