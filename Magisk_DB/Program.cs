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
    // Репозитории будут инициализированы в Main
    private static IDbContextFactory<MagiskHubContext> _dbContextFactory; // Для EnsureCreated
    private static IRoleRepository _roleRepository;
    private static IUserRepository _userRepository;
    private static IModuleCategoryRepository _moduleCategoryRepository;
    private static ITagRepository _tagRepository;
    private static IModuleRepository _moduleRepository;
    private static IModuleVersionRepository _moduleVersionRepository;
    private static IReviewRepository _reviewRepository;
    private static ICompatibilityReportRepository _compatibilityReportRepository;

    private static User _currentUser = null; // Текущий залогиненный пользователь

    static void Main(string[] args) // Синхронный Main
    {
        var services = new ServiceCollection();
        var connectionString = "Host=localhost;Database=Magisk;Username=postgres;Password=1234"; // ВАША СТРОКА

        services.AddDbContextFactory<MagiskHubContext>(options =>
            options.UseNpgsql(connectionString));

        // Регистрация всех ваших синхронных репозиториев
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IModuleCategoryRepository, ModuleCategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IModuleVersionRepository, ModuleVersionRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ICompatibilityReportRepository, CompatibilityReportRepository>();

        var serviceProvider = services.BuildServiceProvider();

        // Получение экземпляров репозиториев
        _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<MagiskHubContext>>();
        _roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        _moduleCategoryRepository = serviceProvider.GetRequiredService<IModuleCategoryRepository>();
        _tagRepository = serviceProvider.GetRequiredService<ITagRepository>();
        _moduleRepository = serviceProvider.GetRequiredService<IModuleRepository>();
        _moduleVersionRepository = serviceProvider.GetRequiredService<IModuleVersionRepository>();
        _reviewRepository = serviceProvider.GetRequiredService<IReviewRepository>();
        _compatibilityReportRepository = serviceProvider.GetRequiredService<ICompatibilityReportRepository>();

        // Убедимся, что БД создана (для разработки)
        using (var context = _dbContextFactory.CreateDbContext()) // Используем фабрику
        {
            context.Database.EnsureCreated(); // Синхронная версия
        }

        Console.WriteLine("Добро пожаловать в MagiskHub Console!");

        while (true)
        {
            try // Общий try-catch для меню, чтобы ловить DataAccessException
            {
                if (_currentUser == null)
                {
                    ShowMainMenuUnauthenticated();
                }
                else
                {
                    ShowMainMenuAuthenticated();
                }
            }
            catch (DataAccessException daEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОШИБКА ДОСТУПА К ДАННЫМ: {daEx.Message}");
                if (daEx.InnerException != null)
                {
                    Console.WriteLine($"Подробнее: {daEx.InnerException.Message}");
                }
                Console.ResetColor();
                Console.WriteLine("Пожалуйста, попробуйте еще раз или обратитесь к администратору.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\nНЕПРЕДВИДЕННАЯ ОШИБКА: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Пожалуйста, попробуйте еще раз или обратитесь к администратору.");
            }
        }
    }

    // --- МЕНЮ ---
    static void ShowMainMenuUnauthenticated()
    {
        Console.WriteLine("\nГлавное меню (Гость):");
        Console.WriteLine("1. Просмотр всех модулей");
        Console.WriteLine("2. Поиск модуля по названию");
        Console.WriteLine("3. Регистрация");
        Console.WriteLine("4. Вход");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите опцию: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1": ViewAllModules(); break;
            case "2": SearchModuleByName(); break;
            case "3": RegisterUser(); break;
            case "4": LoginUser(); break;
            case "0": Environment.Exit(0); break;
            default: Console.WriteLine("Неверный ввод."); break;
        }
    }

    static void ShowMainMenuAuthenticated()
    {
        Console.WriteLine($"\nГлавное меню (Пользователь: {_currentUser.Username} | Роль: {_currentUser.Role.RoleName}):");
        Console.WriteLine("1. Просмотр всех модулей");
        Console.WriteLine("2. Поиск модуля по названию");
        Console.WriteLine("3. Оставить отзыв на модуль");
        Console.WriteLine("4. Сообщить о совместимости версии модуля");

        if (_currentUser.Role.RoleName == "Developer")
        {
            Console.WriteLine("--- Меню Разработчика ---");
            Console.WriteLine("D1. Загрузить новый модуль");
            Console.WriteLine("D2. Добавить новую версию к моему модулю");
            Console.WriteLine("D3. Просмотреть мои модули");
        }
        if (_currentUser.Role.RoleName == "Moderator")
        {
            Console.WriteLine("--- Меню Модератора ---");
            Console.WriteLine("M1. Верифицировать модуль");
            Console.WriteLine("M2. Управление категориями (TODO)"); // Пример для расширения
            Console.WriteLine("M3. Управление тегами (TODO)");      // Пример для расширения
        }

        Console.WriteLine("9. Выйти из аккаунта");
        Console.WriteLine("0. Выход из приложения");
        Console.Write("Выберите опцию: ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1": ViewAllModules(); break;
            case "2": SearchModuleByName(); break;
            case "3": AddReview(); break;
            case "4": ReportCompatibility(); break;
            case "D1": if (_currentUser.Role.RoleName == "Developer") UploadNewModule(); else Console.WriteLine("Доступ запрещен."); break;
            case "D2": if (_currentUser.Role.RoleName == "Developer") AddVersionToMyModule(); else Console.WriteLine("Доступ запрещен."); break;
            case "D3": if (_currentUser.Role.RoleName == "Developer") ViewMyModules(); else Console.WriteLine("Доступ запрещен."); break;
            case "M1": if (_currentUser.Role.RoleName == "Moderator") VerifyModule(); else Console.WriteLine("Доступ запрещен."); break;
            case "M2": if (_currentUser.Role.RoleName == "Moderator") ManageCategoriesMenu(); else Console.WriteLine("Доступ запрещен."); break;
            case "M3": if (_currentUser.Role.RoleName == "Moderator") ManageTagsMenu(); else Console.WriteLine("Доступ запрещен."); break;
            // TODO: M2, M3
            case "9": LogoutUser(); break;
            case "0": Environment.Exit(0); break;
            default: Console.WriteLine("Неверный ввод."); break;
        }
    }
    // --- УПРАВЛЕНИЕ КАТЕГОРИЯМИ (ДЛЯ МОДЕРАТОРА) ---

    static void ManageCategoriesMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Управление категориями ---");
            Console.WriteLine("1. Показать все категории");
            Console.WriteLine("2. Добавить новую категорию");
            Console.WriteLine("3. Редактировать категорию");
            Console.WriteLine("4. Удалить категорию");
            Console.WriteLine("0. Назад в главное меню");
            Console.Write("Выберите опцию: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ListAllCategories(); break;
                case "2": AddNewCategory(); break;
                case "3": EditCategory(); break;
                case "4": DeleteCategory(); break;
                case "0": return; // Возврат в предыдущее меню
                default: Console.WriteLine("Неверный ввод."); break;
            }
        }
    }

    static void ListAllCategories()
    {
        var categories = _moduleCategoryRepository.GetAll();
        if (!categories.Any()) { Console.WriteLine("Категорий нет."); return; }
        Console.WriteLine("\n--- Список категорий ---");
        foreach (var c in categories) { Console.WriteLine($"ID: {c.CategoryID}, Название: {c.CategoryName}, Описание: {c.Description ?? "N/A"}"); }
    }

    static void AddNewCategory()
    {
        Console.Write("Название новой категории: "); string name = Console.ReadLine();
        Console.Write("Описание (необязательно): "); string desc = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Название не может быть пустым."); return; }
        if (_moduleCategoryRepository.GetByName(name) != null) { Console.WriteLine("Категория с таким названием уже существует."); return; }
        _moduleCategoryRepository.Add(new ModuleCategory { CategoryName = name, Description = desc });
        Console.WriteLine("Категория добавлена.");
    }

    static void EditCategory()
    {
        ListAllCategories();
        Console.Write("ID категории для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }
        var cat = _moduleCategoryRepository.GetById(id);
        if (cat == null) { Console.WriteLine("Категория не найдена."); return; }

        Console.Write($"Новое название для '{cat.CategoryName}' (Enter, чтобы не менять): "); string newName = Console.ReadLine();
        Console.Write($"Новое описание (Enter, чтобы не менять): "); string newDesc = Console.ReadLine();
        bool changed = false;
        if (!string.IsNullOrWhiteSpace(newName) && newName != cat.CategoryName)
        {
            if (_moduleCategoryRepository.GetByName(newName) != null && _moduleCategoryRepository.GetByName(newName).CategoryID != id)
            { Console.WriteLine("Другая категория с таким названием уже существует."); return; }
            cat.CategoryName = newName; changed = true;
        }
        if (newDesc != cat.Description) // Позволяем очистить описание
        {
            cat.Description = string.IsNullOrWhiteSpace(newDesc) ? null : newDesc;
            changed = true;
        }
        if (changed) { _moduleCategoryRepository.Update(cat); Console.WriteLine("Категория обновлена."); }
        else { Console.WriteLine("Нет изменений."); }
    }

    static void DeleteCategory()
    {
        ListAllCategories();
        Console.Write("ID категории для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }
        var cat = _moduleCategoryRepository.GetById(id); // Нужна для имени
        if (cat == null) { Console.WriteLine("Категория не найдена."); return; }

        if (_moduleCategoryRepository.IsCategoryInUse(id))
        { Console.WriteLine($"Категория '{cat.CategoryName}' используется модулями и не может быть удалена."); return; }

        Console.Write($"Удалить категорию '{cat.CategoryName}'? (yes/no): ");
        if (Console.ReadLine().ToLower() == "yes")
        {
            if (_moduleCategoryRepository.Delete(id)) Console.WriteLine("Категория удалена.");
            else Console.WriteLine("Ошибка при удалении или категория не найдена.");
        }
        else { Console.WriteLine("Удаление отменено."); }
    }


    // --- УПРАВЛЕНИЕ ТЕГАМИ (ДЛЯ МОДЕРАТОРА) ---

    static void ManageTagsMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Управление тегами ---");
            Console.WriteLine("1. Показать все теги");
            Console.WriteLine("2. Добавить новый тег");
            Console.WriteLine("3. Редактировать тег");
            Console.WriteLine("4. Удалить тег");
            Console.WriteLine("0. Назад в главное меню");
            Console.Write("Выберите опцию: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ListAllTags(); break;
                case "2": AddNewTag(); break;
                case "3": EditTag(); break;
                case "4": DeleteGlobalTag(); break;
                case "0": return;
                default: Console.WriteLine("Неверный ввод."); break;
            }
        }
    }

    static void ListAllTags()
    {
        var tags = _tagRepository.GetAll();
        if (!tags.Any()) { Console.WriteLine("Тегов нет."); return; }
        Console.WriteLine("\n--- Список тегов ---");
        foreach (var t in tags) { Console.WriteLine($"ID: {t.TagID}, Название: {t.TagName}"); }
    }
    static void AddNewTag()
    {
        Console.Write("Название нового тега: "); string name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Название не может быть пустым."); return; }
        if (_tagRepository.GetByName(name) != null) { Console.WriteLine("Тег с таким названием уже существует."); return; }
        _tagRepository.Add(new Tag { TagName = name });
        Console.WriteLine("Тег добавлен.");
    }
    static void EditTag()
    {
        ListAllTags();
        Console.Write("ID тега для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }
        var tag = _tagRepository.GetById(id);
        if (tag == null) { Console.WriteLine("Тег не найден."); return; }

        Console.Write($"Новое название для '{tag.TagName}' (Enter, чтобы не менять): "); string newName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newName) && newName != tag.TagName)
        {
            if (_tagRepository.GetByName(newName) != null && _tagRepository.GetByName(newName).TagID != id)
            { Console.WriteLine("Другой тег с таким названием уже существует."); return; }
            tag.TagName = newName;
            _tagRepository.Update(tag);
            Console.WriteLine("Тег обновлен.");
        }
        else { Console.WriteLine("Нет изменений."); }
    }
    static void DeleteGlobalTag() // Переименовано
    {
        ListAllTags();
        Console.Write("ID тега для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }
        var tag = _tagRepository.GetById(id);
        if (tag == null) { Console.WriteLine("Тег не найден."); return; }

        if (_tagRepository.IsTagInUse(id))
        { Console.WriteLine($"Тег '{tag.TagName}' используется модулями и не может быть удален."); return; }

        Console.Write($"Удалить тег '{tag.TagName}'? (yes/no): ");
        if (Console.ReadLine().ToLower() == "yes")
        {
            if (_tagRepository.Delete(id)) Console.WriteLine("Тег удален.");
            else Console.WriteLine("Ошибка при удалении или тег не найден.");
        }
        else { Console.WriteLine("Удаление отменено."); }
    }

    static void ViewAllModules()
    {
        var modules = _moduleRepository.GetAllModulesWithAuthorAndCategory();
        if (!modules.Any()) { Console.WriteLine("Модулей пока нет."); return; }
        Console.WriteLine("\n--- Список модулей ---");
        foreach (var m in modules)
        {
            Console.WriteLine($"ID: {m.ModuleID}, Название: {m.Name}, Автор: {m.Author?.Username ?? "N/A"}, Категория: {m.Category?.CategoryName ?? "N/A"}, Верифицирован: {m.IsVerified}");
        }
    }

    static void ViewModuleDetails()
    {
        Console.Write("Введите ID модуля для просмотра деталей: ");
        if (!int.TryParse(Console.ReadLine(), out int moduleId)) { Console.WriteLine("Некорректный ID."); return; }

        var module = _moduleRepository.GetModuleByIdWithDetails(moduleId);
        if (module == null) { Console.WriteLine("Модуль не найден."); return; }

        Console.WriteLine($"\n--- Детали модуля: {module.Name} (ID: {module.ModuleID}) ---");
        Console.WriteLine($"Описание: {module.Description}");
        Console.WriteLine($"Автор: {module.Author?.Username ?? "N/A"}");
        Console.WriteLine($"Категория: {module.Category?.CategoryName ?? "N/A"}");
        Console.WriteLine($"Верифицирован: {module.IsVerified}");
        Console.WriteLine($"Создан: {module.CreationDate}, Обновлен: {module.LastUpdateDate}");

        if (module.Versions.Any())
        {
            Console.WriteLine("Версии:");
            foreach (var v in module.Versions.OrderByDescending(v => v.UploadDate))
            {
                Console.WriteLine($"  - {v.VersionString} (ID: {v.VersionID}, Загружено: {v.UploadDate.ToShortDateString()})");
                Console.WriteLine($"    Ссылка: {v.DownloadLink}");
                if (!string.IsNullOrEmpty(v.Changelog)) Console.WriteLine($"    Изменения: {v.Changelog}");
            }
        }
        else { Console.WriteLine("Версий нет."); }

        var reviews = _reviewRepository.GetReviewsForModule(moduleId);
        if (reviews.Any())
        {
            Console.WriteLine("Отзывы:");
            foreach (var r in reviews)
            {
                Console.WriteLine($"  - {r.User?.Username ?? "Аноним"}: {r.Rating}/5 \"{r.CommentText ?? ""}\" ({r.ReviewDate.ToShortDateString()})");
            }
        }
        else { Console.WriteLine("Отзывов нет."); }

        if (module.ModuleTags.Any())
        {
            Console.WriteLine($"Теги: {string.Join(", ", module.ModuleTags.Select(mt => mt.Tag.TagName))}");
        }
        else { Console.WriteLine("Тегов нет."); }
    }


    // --- ПРОСТОЕ ХЭШИРОВАНИЕ (НЕ ДЛЯ ПРОДА!) ---
    static string SimpleHashPassword(string password)
    {
        // ВНИМАНИЕ: Это НЕ безопасный способ хэширования. Используйте BCrypt.Net или Argon2.
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // --- РЕГИСТРАЦИЯ И ВХОД ---
    static void RegisterUser()
    {
        Console.Write("Введите имя пользователя: "); string username = Console.ReadLine();
        Console.Write("Введите email: "); string email = Console.ReadLine();
        Console.Write("Введите пароль: "); string password = Console.ReadLine(); // TODO: Скрывать ввод

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        { Console.WriteLine("Все поля обязательны."); return; }

        if (_userRepository.GetUserByUsername(username) != null)
        { Console.WriteLine("Пользователь с таким именем уже существует."); return; }
        if (_userRepository.GetUserByEmail(email) != null)
        { Console.WriteLine("Пользователь с таким email уже существует."); return; }

        var defaultRole = _roleRepository.GetRoleByName("EndUser");
        if (defaultRole == null)
        {
            defaultRole = _roleRepository.GetAllRoles().FirstOrDefault(); // Аварийный вариант
            if (defaultRole == null) { Console.WriteLine("Ошибка: Роли не найдены."); return; }
            Console.WriteLine($"Предупреждение: Роль 'EndUser' не найдена, назначена роль '{defaultRole.RoleName}'.");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = SimpleHashPassword(password),
            RoleID = defaultRole.RoleID,
            RegistrationDate = DateTime.UtcNow
        };
        _userRepository.AddUser(user);
        Console.WriteLine("Регистрация успешна!");
    }

    static void LoginUser()
    {
        Console.Write("Введите имя пользователя: "); string username = Console.ReadLine();
        Console.Write("Введите пароль: "); string password = Console.ReadLine();

        var user = _userRepository.GetUserByUsernameWithRole(username);
        if (user != null && user.PasswordHash == SimpleHashPassword(password))
        {
            _currentUser = user;
            Console.WriteLine($"Вход успешен! Добро пожаловать, {user.Username}!");
        }
        else { Console.WriteLine("Неверное имя пользователя или пароль."); }
    }

    static void LogoutUser()
    {
        if (_currentUser != null) Console.WriteLine($"Пользователь {_currentUser.Username} вышел из системы.");
        _currentUser = null;
    }

    static void SearchModuleByName()
    {
        Console.Write("Введите часть названия модуля для поиска: "); string searchTerm = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(searchTerm)) { Console.WriteLine("Запрос не может быть пустым."); return; }

        var modules = _moduleRepository.SearchModulesByName(searchTerm);
        if (!modules.Any()) { Console.WriteLine($"Модули по запросу '{searchTerm}' не найдены."); return; }
        Console.WriteLine($"\n--- Результаты поиска по '{searchTerm}' ---");
        foreach (var m in modules)
        {
            Console.WriteLine($"ID: {m.ModuleID}, Название: {m.Name}, Автор: {m.Author?.Username ?? "N/A"}, Категория: {m.Category?.CategoryName ?? "N/A"}");
        }
    }

    static void AddReview()
    {
        if (_currentUser == null) { Console.WriteLine("Сначала войдите в систему."); return; }
        ViewAllModules(); // Показать модули для выбора ID
        Console.Write("Введите ID модуля для отзыва: ");
        if (!int.TryParse(Console.ReadLine(), out int moduleId)) { Console.WriteLine("Некорректный ID модуля."); return; }

        var module = _moduleRepository.GetModuleById(moduleId);
        if (module == null) { Console.WriteLine("Модуль не найден."); return; }

        Console.Write("Ваша оценка (1-5): ");
        if (!int.TryParse(Console.ReadLine(), out int rating) || rating < 1 || rating > 5) { Console.WriteLine("Некорректная оценка."); return; }
        Console.Write("Ваш комментарий: "); string comment = Console.ReadLine();

        var review = new Review
        {
            ModuleID = moduleId,
            UserID = _currentUser.UserID,
            Rating = rating,
            CommentText = comment,
            ReviewDate = DateTime.UtcNow
        };
        _reviewRepository.Add(review);
        Console.WriteLine("Отзыв успешно добавлен!");
    }


    static void ReportCompatibility()
    {
        if (_currentUser == null) { Console.WriteLine("Сначала войдите в систему."); return; }
        // Сначала нужно выбрать модуль, потом версию
        ViewAllModules();
        Console.Write("Введите ID модуля, для версии которого хотите оставить отчет: ");
        if (!int.TryParse(Console.ReadLine(), out int moduleIdSelect)) { Console.WriteLine("Некорректный ID модуля."); return; }
        var moduleWithVersions = _moduleRepository.GetModuleByIdWithDetails(moduleIdSelect);
        if (moduleWithVersions == null || !moduleWithVersions.Versions.Any()) { Console.WriteLine("Модуль не найден или у него нет версий."); return; }

        Console.WriteLine("Доступные версии для модуля " + moduleWithVersions.Name + ":");
        foreach (var v in moduleWithVersions.Versions) { Console.WriteLine($" ID Версии: {v.VersionID} - {v.VersionString}"); }

        Console.Write("Введите ID версии модуля: ");
        if (!int.TryParse(Console.ReadLine(), out int moduleVersionId)) { Console.WriteLine("Некорректный ID версии."); return; }

        var moduleVersion = _moduleVersionRepository.GetById(moduleVersionId);
        if (moduleVersion == null || moduleVersion.ModuleID != moduleIdSelect) { Console.WriteLine("Версия модуля не найдена или не принадлежит выбранному модулю."); return; }

        Console.Write("Модель вашего устройства: "); string deviceModel = Console.ReadLine();
        Console.Write("Версия Android: "); string androidVersion = Console.ReadLine();
        Console.Write("Статус работы (Works/WorksWithIssues/DoesNotWork): "); string worksStatus = Console.ReadLine();
        Console.Write("Дополнительные заметки (необязательно): "); string notes = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(deviceModel) || string.IsNullOrWhiteSpace(androidVersion) || string.IsNullOrWhiteSpace(worksStatus))
        { Console.WriteLine("Модель, версия Android и статус должны быть заполнены."); return; }

        var report = new CompatibilityReport
        {
            ModuleVersionID = moduleVersionId,
            UserID = _currentUser.UserID,
            DeviceModel = deviceModel,
            AndroidVersion = androidVersion,
            WorksStatus = worksStatus,
            UserNotes = notes,
            ReportDate = DateTime.UtcNow
        };
        _compatibilityReportRepository.Add(report);
        Console.WriteLine("Отчет о совместимости успешно добавлен!");
    }


    // --- ДЕЙСТВИЯ ДЛЯ РАЗРАБОТЧИКА ---
    static void UploadNewModule()
    {
        if (_currentUser?.Role?.RoleName != "Developer") { Console.WriteLine("Доступ запрещен."); return; }

        Console.Write("Название нового модуля: "); string name = Console.ReadLine();
        Console.Write("Описание модуля: "); string description = Console.ReadLine();

        var categories = _moduleCategoryRepository.GetAll();
        if (!categories.Any()) { Console.WriteLine("Категории не найдены. Модератор должен их добавить."); return; }
        Console.WriteLine("Доступные категории:");
        foreach (var cat in categories) { Console.WriteLine($"{cat.CategoryID}. {cat.CategoryName}"); }
        Console.Write("Выберите ID категории: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId) || !_moduleCategoryRepository.GetAll().Any(c => c.CategoryID == categoryId))
        { Console.WriteLine("Некорректный ID категории."); return; }

        Console.Write("Версия модуля (например, v1.0): "); string versionString = Console.ReadLine();
        Console.Write("Ссылка на скачивание файла модуля: "); string downloadLink = Console.ReadLine();
        Console.Write("Changelog (можно оставить пустым): "); string changelog = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(versionString) || string.IsNullOrWhiteSpace(downloadLink))
        { Console.WriteLine("Название, описание, версия и ссылка на скачивание обязательны."); return; }

        var newModule = new Module
        {
            Name = name,
            Description = description,
            AuthorUserID = _currentUser.UserID,
            CategoryID = categoryId,
            IsVerified = false,
            CreationDate = DateTime.UtcNow,
            LastUpdateDate = DateTime.UtcNow
        };
        var newVersion = new ModuleVersion // Module будет присвоен в AddModuleWithFirstVersion
        {
            VersionString = versionString,
            DownloadLink = downloadLink,
            Changelog = changelog,
            UploadDate = DateTime.UtcNow
        };

        _moduleRepository.AddModuleWithFirstVersion(newModule, newVersion);
        Console.WriteLine($"Модуль '{name}' и его версия '{versionString}' успешно загружены и ожидают верификации.");
    }

    static void AddVersionToMyModule()
    {
        if (_currentUser?.Role?.RoleName != "Developer") { Console.WriteLine("Доступ запрещен."); return; }
        var myModules = _moduleRepository.GetModulesByAuthor(_currentUser.UserID);
        if (!myModules.Any()) { Console.WriteLine("У вас пока нет загруженных модулей."); return; }

        Console.WriteLine("Ваши модули:");
        foreach (var mod in myModules) { Console.WriteLine($"ID: {mod.ModuleID}, Название: {mod.Name}"); }
        Console.Write("Введите ID модуля для добавления версии: ");
        if (!int.TryParse(Console.ReadLine(), out int moduleId) || !myModules.Any(m => m.ModuleID == moduleId))
        { Console.WriteLine("Некорректный ID модуля или модуль вам не принадлежит."); return; }

        Console.Write("Новая версия (например, v1.1): "); string versionString = Console.ReadLine();
        Console.Write("Ссылка на скачивание: "); string downloadLink = Console.ReadLine();
        Console.Write("Changelog: "); string changelog = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(versionString) || string.IsNullOrWhiteSpace(downloadLink))
        { Console.WriteLine("Версия и ссылка на скачивание обязательны."); return; }

        var newVersion = new ModuleVersion
        {
            ModuleID = moduleId,
            VersionString = versionString,
            DownloadLink = downloadLink,
            Changelog = changelog,
            UploadDate = DateTime.UtcNow
        };
        _moduleVersionRepository.Add(newVersion);

        var moduleToUpdate = _moduleRepository.GetModuleById(moduleId); // Получаем для обновления даты
        if (moduleToUpdate != null)
        {
            moduleToUpdate.LastUpdateDate = DateTime.UtcNow;
            _moduleRepository.UpdateModule(moduleToUpdate);
        }
        Console.WriteLine($"Новая версия '{versionString}' для модуля ID {moduleId} успешно добавлена.");
    }

    static void ViewMyModules()
    {
        if (_currentUser?.Role?.RoleName != "Developer") { Console.WriteLine("Доступ запрещен."); return; }
        var myModules = _moduleRepository.GetModulesByAuthor(_currentUser.UserID); // GetModulesByAuthor уже включает версии
        if (!myModules.Any()) { Console.WriteLine("У вас пока нет загруженных модулей."); return; }

        Console.WriteLine("\n--- Ваши модули ---");
        foreach (var mod in myModules)
        {
            Console.WriteLine($"ID: {mod.ModuleID}, Название: {mod.Name}, Категория: {mod.Category?.CategoryName ?? "N/A"}, Верифицирован: {mod.IsVerified}");
            if (mod.Versions.Any())
            {
                Console.WriteLine("  Версии:");
                foreach (var ver in mod.Versions.OrderByDescending(v => v.UploadDate))
                { Console.WriteLine($"    - {ver.VersionString} (Загружено: {ver.UploadDate.ToShortDateString()})"); }
            }
            else { Console.WriteLine("  (Версий нет)"); }
        }
    }

    static void ManageMyModuleTags()
    {
        if (_currentUser?.Role?.RoleName != "Developer") { Console.WriteLine("Доступ запрещен."); return; }
        var myModules = _moduleRepository.GetModulesByAuthor(_currentUser.UserID);
        if (!myModules.Any()) { Console.WriteLine("У вас нет модулей для управления тегами."); return; }

        Console.WriteLine("Ваши модули:");
        foreach (var mod in myModules) { Console.WriteLine($"ID: {mod.ModuleID} - {mod.Name}"); }
        Console.Write("Введите ID модуля для управления тегами: ");
        if (!int.TryParse(Console.ReadLine(), out int moduleId) || !myModules.Any(m => m.ModuleID == moduleId))
        { Console.WriteLine("Неверный ID или модуль не ваш."); return; }

        var moduleWithTags = _moduleRepository.GetModuleByIdWithDetails(moduleId); // Чтобы видеть текущие теги
        Console.WriteLine($"Текущие теги для '{moduleWithTags.Name}': " +
                          (moduleWithTags.ModuleTags.Any() ? string.Join(", ", moduleWithTags.ModuleTags.Select(mt => mt.Tag.TagName)) : "Нет"));

        Console.WriteLine("1. Добавить тег к модулю");
        Console.WriteLine("2. Удалить тег с модуля");
        Console.Write("Выберите действие: ");
        string choice = Console.ReadLine();

        var allTags = _tagRepository.GetAll().ToList();
        if (!allTags.Any()) { Console.WriteLine("В системе нет тегов. Модератор должен их добавить."); return; }

        Console.WriteLine("Доступные теги в системе:");
        foreach (var tag in allTags) { Console.WriteLine($"ID: {tag.TagID} - {tag.TagName}"); }

        if (choice == "1")
        {
            Console.Write("Введите ID тега для добавления: ");
            if (int.TryParse(Console.ReadLine(), out int tagIdAdd) && allTags.Any(t => t.TagID == tagIdAdd))
            {
                _moduleRepository.AddTagToModule(moduleId, tagIdAdd);
                Console.WriteLine("Тег добавлен (если его еще не было).");
            }
            else { Console.WriteLine("Неверный ID тега."); }
        }
        else if (choice == "2")
        {
            Console.Write("Введите ID тега для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int tagIdRemove) && allTags.Any(t => t.TagID == tagIdRemove))
            {
                _moduleRepository.RemoveTagFromModule(moduleId, tagIdRemove);
                Console.WriteLine("Тег удален (если он был).");
            }
            else { Console.WriteLine("Неверный ID тега."); }
        }
        else { Console.WriteLine("Неверный выбор."); }
    }

    // --- ДЕЙСТВИЯ ДЛЯ МОДЕРАТОРА ---
    static void VerifyModule()
    {
        if (_currentUser?.Role?.RoleName != "Moderator") { Console.WriteLine("Доступ запрещен."); return; }
        var unverifiedModules = _moduleRepository.GetUnverifiedModules();
        if (!unverifiedModules.Any()) { Console.WriteLine("Нет модулей, ожидающих верификации."); return; }

        Console.WriteLine("\n--- Модули, ожидающие верификации ---");
        foreach (var mod in unverifiedModules) { Console.WriteLine($"ID: {mod.ModuleID}, Название: {mod.Name}, Автор: {mod.Author?.Username ?? "N/A"}"); }
        Console.Write("Введите ID модуля для верификации (или 0 для отмены): ");
        if (!int.TryParse(Console.ReadLine(), out int moduleId) || moduleId == 0) { return; }

        var moduleToVerify = _moduleRepository.GetModuleById(moduleId); // Нам нужна отслеживаемая сущность
        if (moduleToVerify == null || moduleToVerify.IsVerified)
        { Console.WriteLine("Модуль не найден или уже верифицирован."); return; }

        Console.Write($"Верифицировать модуль '{moduleToVerify.Name}'? (yes/no): ");
        if (Console.ReadLine().ToLower() == "yes")
        {
            moduleToVerify.IsVerified = true;
            moduleToVerify.LastUpdateDate = DateTime.UtcNow;
            _moduleRepository.UpdateModule(moduleToVerify); // Передаем измененную сущность
            Console.WriteLine($"Модуль '{moduleToVerify.Name}' успешно верифицирован.");
        }
        else { Console.WriteLine("Верификация отменена."); }
    }

    static void ManageGlobalTagsMenu() // Переименовано для ясности
    {
        if (_currentUser?.Role?.RoleName != "Moderator") { Console.WriteLine("Доступ запрещен."); return; }
        while (true)
        {
            Console.WriteLine("\n--- Управление тегами (Глобально) ---");
            Console.WriteLine("1. Показать все теги");
            Console.WriteLine("2. Добавить новый тег");
            Console.WriteLine("3. Редактировать тег");
            Console.WriteLine("4. Удалить тег");
            Console.WriteLine("0. Назад");
            Console.Write("Выберите опцию: "); string choice = Console.ReadLine();
            switch (choice)
            {
                case "1": ListAllTags(); break;
                case "2": AddNewTag(); break;
                case "3": EditTag(); break;
                case "4": DeleteGlobalTag(); break; // Переименовано для ясности
                case "0": return;
                default: Console.WriteLine("Неверный ввод."); break;
            }
        }
    }
}

