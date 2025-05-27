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
    private static DbContextOptions<MagiskHubContext> _dbContextOptions;
    private static User _currentUser = null; // Текущий залогиненный пользователь

    static void Main(string[] args)
    {
        // Настройка подключения к БД (PostgreSQL в вашем случае)
        var connectionString = "Host=localhost;Database=Magisk;Username=postgres;Password=1234"; // ВАША СТРОКА ПОДКЛЮЧЕНИЯ
        _dbContextOptions = new DbContextOptionsBuilder<MagiskHubContext>()
            .UseNpgsql(connectionString)
            .Options;

        // Убедимся, что БД создана (для разработки)
        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            context.Database.EnsureCreated();
        }

        Console.WriteLine("Добро пожаловать в MagiskHub Console!");

        while (true)
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
        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var categories = context.ModuleCategories.ToList();
            if (!categories.Any())
            {
                Console.WriteLine("Категорий пока нет.");
                return;
            }
            Console.WriteLine("\n--- Список категорий ---");
            foreach (var cat in categories)
            {
                Console.WriteLine($"ID: {cat.CategoryID}, Название: {cat.CategoryName}, Описание: {cat.Description ?? "N/A"}");
            }
        }
    }

    static void AddNewCategory()
    {
        Console.Write("Введите название новой категории: ");
        string name = Console.ReadLine();
        Console.Write("Введите описание категории (необязательно): ");
        string description = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Название категории не может быть пустым.");
            return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            if (context.ModuleCategories.Any(mc => mc.CategoryName.ToLower() == name.ToLower()))
            {
                Console.WriteLine("Категория с таким названием уже существует.");
                return;
            }

            var newCategory = new ModuleCategory { CategoryName = name, Description = description };
            try
            {
                context.ModuleCategories.Add(newCategory);
                context.SaveChanges();
                Console.WriteLine($"Категория '{name}' успешно добавлена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении категории: {ex.Message}");
            }
        }
    }

    static void EditCategory()
    {
        ListAllCategories(); // Показать список для удобства выбора ID
        Console.Write("Введите ID категории для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            Console.WriteLine("Некорректный ID."); return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var category = context.ModuleCategories.Find(categoryId);
            if (category == null)
            {
                Console.WriteLine("Категория с таким ID не найдена."); return;
            }

            Console.Write($"Новое название для '{category.CategoryName}' (оставьте пустым, чтобы не менять): ");
            string newName = Console.ReadLine();
            Console.Write($"Новое описание для '{category.CategoryName}' (оставьте пустым, чтобы не менять): ");
            string newDescription = Console.ReadLine();

            bool changed = false;
            if (!string.IsNullOrWhiteSpace(newName) && newName != category.CategoryName)
            {
                // Проверка на уникальность нового имени, если оно меняется
                if (context.ModuleCategories.Any(mc => mc.CategoryName.ToLower() == newName.ToLower() && mc.CategoryID != categoryId))
                {
                    Console.WriteLine("Категория с таким новым названием уже существует.");
                    return;
                }
                category.CategoryName = newName;
                changed = true;
            }
            if (!string.IsNullOrWhiteSpace(newDescription) && newDescription != category.Description)
            {
                category.Description = newDescription;
                changed = true;
            }
            // Можно было бы и пустую строку в описание разрешить, если оно было не пустым
            if (string.IsNullOrWhiteSpace(newDescription) && !string.IsNullOrWhiteSpace(category.Description))
            {
                category.Description = null; // Если пользователь ввел пустое, а было что-то - очищаем
                changed = true;
            }


            if (changed)
            {
                try
                {
                    context.SaveChanges();
                    Console.WriteLine("Категория успешно обновлена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении категории: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Нет изменений для сохранения.");
            }
        }
    }

    static void DeleteCategory()
    {
        ListAllCategories();
        Console.Write("Введите ID категории для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            Console.WriteLine("Некорректный ID."); return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var category = context.ModuleCategories.Include(c => c.Modules).FirstOrDefault(c => c.CategoryID == categoryId);
            if (category == null)
            {
                Console.WriteLine("Категория с таким ID не найдена."); return;
            }

            if (category.Modules.Any())
            {
                Console.WriteLine($"Нельзя удалить категорию '{category.CategoryName}', так как она используется модулями (ID модулей: {string.Join(", ", category.Modules.Select(m => m.ModuleID))}).");
                Console.WriteLine("Сначала измените категорию у этих модулей.");
                return;
            }

            Console.Write($"Вы уверены, что хотите удалить категорию '{category.CategoryName}'? (yes/no): ");
            if (Console.ReadLine().ToLower() == "yes")
            {
                try
                {
                    context.ModuleCategories.Remove(category);
                    context.SaveChanges();
                    Console.WriteLine("Категория успешно удалена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при удалении категории: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Удаление отменено.");
            }
        }
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
                case "4": DeleteTag(); break;
                case "0": return;
                default: Console.WriteLine("Неверный ввод."); break;
            }
        }
    }

    static void ListAllTags()
    {
        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var tags = context.Tags.ToList();
            if (!tags.Any())
            {
                Console.WriteLine("Тегов пока нет.");
                return;
            }
            Console.WriteLine("\n--- Список тегов ---");
            foreach (var tag in tags)
            {
                Console.WriteLine($"ID: {tag.TagID}, Название: {tag.TagName}");
            }
        }
    }

    static void AddNewTag()
    {
        Console.Write("Введите название нового тега: ");
        string name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Название тега не может быть пустым.");
            return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            if (context.Tags.Any(t => t.TagName.ToLower() == name.ToLower()))
            {
                Console.WriteLine("Тег с таким названием уже существует.");
                return;
            }

            var newTag = new Tag { TagName = name };
            try
            {
                context.Tags.Add(newTag);
                context.SaveChanges();
                Console.WriteLine($"Тег '{name}' успешно добавлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении тега: {ex.Message}");
            }
        }
    }

    static void EditTag()
    {
        ListAllTags();
        Console.Write("Введите ID тега для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out int tagId))
        {
            Console.WriteLine("Некорректный ID."); return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var tag = context.Tags.Find(tagId);
            if (tag == null)
            {
                Console.WriteLine("Тег с таким ID не найден."); return;
            }

            Console.Write($"Новое название для '{tag.TagName}' (оставьте пустым, чтобы не менять): ");
            string newName = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(newName) && newName.ToLower() != tag.TagName.ToLower())
            {
                if (context.Tags.Any(t => t.TagName.ToLower() == newName.ToLower() && t.TagID != tagId))
                {
                    Console.WriteLine("Тег с таким новым названием уже существует.");
                    return;
                }
                tag.TagName = newName;
                try
                {
                    context.SaveChanges();
                    Console.WriteLine("Тег успешно обновлен.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении тега: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Нет изменений для сохранения или новое имя совпадает со старым.");
            }
        }
    }

    static void DeleteTag()
    {
        ListAllTags();
        Console.Write("Введите ID тега для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int tagId))
        {
            Console.WriteLine("Некорректный ID."); return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            // Проверяем, используется ли тег в связующей таблице ModuleTags
            var tag = context.Tags.Include(t => t.ModuleTags).FirstOrDefault(t => t.TagID == tagId);

            if (tag == null)
            {
                Console.WriteLine("Тег с таким ID не найден."); return;
            }

            if (tag.ModuleTags.Any())
            {
                Console.WriteLine($"Нельзя удалить тег '{tag.TagName}', так как он используется модулями.");
                Console.WriteLine("Сначала удалите этот тег у всех модулей, которые его используют.");
                // Можно было бы показать ID модулей, но это требует еще одного запроса
                return;
            }

            Console.Write($"Вы уверены, что хотите удалить тег '{tag.TagName}'? (yes/no): ");
            if (Console.ReadLine().ToLower() == "yes")
            {
                try
                {
                    context.Tags.Remove(tag);
                    context.SaveChanges();
                    Console.WriteLine("Тег успешно удален.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при удалении тега: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Удаление отменено.");
            }
        }
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
        Console.Write("Введите имя пользователя: ");
        string username = Console.ReadLine();
        Console.Write("Введите email: ");
        string email = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Имя пользователя, email и пароль не могут быть пустыми.");
            return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            if (context.Users.Any(u => u.Username == username))
            {
                Console.WriteLine("Пользователь с таким именем уже существует.");
                return;
            }
            if (context.Users.Any(u => u.Email == email))
            {
                Console.WriteLine("Пользователь с таким email уже существует.");
                return;
            }

            // Для тестов
            // Для тестов

            Console.WriteLine("Введите нужную роль: \n" +
                "1 - EndUser\n" +
                "2 - Developer\n" +
                "3 - Moderator");
            string testFeature = Console.ReadLine();
            string testFeature2 = "EndUser";

            switch (testFeature)
            {
                case "1":
                    testFeature2 = "EndUser";
                    break;
                case "2":
                    testFeature2 = "Developer";
                    break;
                case "3":
                    testFeature2 = "Moderator";
                    break;
            }
            

            var defaultRole = context.Roles.FirstOrDefault(r => r.RoleName == testFeature2);
            if (defaultRole == null) // Если роли EndUser нет, создадим (или используем ID=1 если есть seed)
            {
                // Этого не должно быть, если есть SeedData
                Console.WriteLine("Ошибка: Роль EndUser не найдена. Обратитесь к администратору.");
                return;
            }
            // Для тестов
            // Для тестов


            // По умолчанию регистрируем как EndUser

            //var defaultRole = context.Roles.FirstOrDefault(r => r.RoleName == "EndUser");
            //if (defaultRole == null) // Если роли EndUser нет, создадим (или используем ID=1 если есть seed)
            //{
            //    // Этого не должно быть, если есть SeedData
            //    Console.WriteLine("Ошибка: Роль EndUser не найдена. Обратитесь к администратору.");
            //    return;
            //}

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = SimpleHashPassword(password),
                RoleID = defaultRole.RoleID,
                RegistrationDate = DateTime.UtcNow
            };

            try
            {
                context.Users.Add(user);
                context.SaveChanges();
                Console.WriteLine("Регистрация успешна!");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Ошибка при регистрации: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }

    static void LoginUser()
    {
        Console.Write("Введите имя пользователя: ");
        string username = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var user = context.Users
                              .Include(u => u.Role) // Важно подгрузить роль
                              .FirstOrDefault(u => u.Username == username);

            if (user != null && user.PasswordHash == SimpleHashPassword(password))
            {
                _currentUser = user;
                Console.WriteLine($"Вход успешен! Добро пожаловать, {user.Username}!");
            }
            else
            {
                Console.WriteLine("Неверное имя пользователя или пароль.");
            }
        }
    }

    static void LogoutUser()
    {
        Console.WriteLine($"Пользователь {_currentUser.Username} вышел из системы.");
        _currentUser = null;
    }

    // --- ДЕЙСТВИЯ С МОДУЛЯМИ ---
    static void ViewAllModules()
    {
        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var modules = context.Modules
                                .Include(m => m.Author)
                                .Include(m => m.Category)
                                .ToList();

            if (!modules.Any())
            {
                Console.WriteLine("Модулей пока нет.");
                return;
            }

            Console.WriteLine("\n--- Список модулей ---");
            foreach (var module in modules)
            {
                Console.WriteLine($"ID: {module.ModuleID}, Название: {module.Name}, Автор: {module.Author.Username}, Категория: {module.Category.CategoryName}, Верифицирован: {module.IsVerified}");
                // Можно добавить вывод версий и среднего рейтинга, если нужно
            }
        }
    }

    static void SearchModuleByName()
    {
        Console.Write("Введите часть названия модуля для поиска: ");
        string searchTerm = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            Console.WriteLine("Поисковый запрос не может быть пустым.");
            return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var modules = context.Modules
                                .Include(m => m.Author)
                                .Include(m => m.Category)
                                .Where(m => m.Name.ToLower().Contains(searchTerm.ToLower()))
                                .ToList();

            if (!modules.Any())
            {
                Console.WriteLine($"Модули по запросу '{searchTerm}' не найдены.");
                return;
            }

            Console.WriteLine($"\n--- Результаты поиска по '{searchTerm}' ---");
            foreach (var module in modules)
            {
                Console.WriteLine($"ID: {module.ModuleID}, Название: {module.Name}, Автор: {module.Author.Username}, Категория: {module.Category.CategoryName}");
            }
        }
    }

    static void AddReview()
    {
        if (_currentUser == null) { Console.WriteLine("Сначала войдите в систему."); return; }

        Console.Write("Введите ID модуля для отзыва: ");
        if (!int.TryParse(Console.ReadLine(), out int moduleId))
        {
            Console.WriteLine("Некорректный ID модуля."); return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var module = context.Modules.Find(moduleId);
            if (module == null) { Console.WriteLine("Модуль не найден."); return; }

            Console.Write("Ваша оценка (1-5): ");
            if (!int.TryParse(Console.ReadLine(), out int rating) || rating < 1 || rating > 5)
            {
                Console.WriteLine("Некорректная оценка."); return;
            }
            Console.Write("Ваш комментарий: ");
            string comment = Console.ReadLine();

            var review = new Review
            {
                ModuleID = moduleId,
                UserID = _currentUser.UserID,
                Rating = rating,
                CommentText = comment,
                ReviewDate = DateTime.UtcNow
            };

            try
            {
                context.Reviews.Add(review);
                context.SaveChanges();
                Console.WriteLine("Отзыв успешно добавлен!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении отзыва: {ex.Message}");
            }
        }
    }

    static void ReportCompatibility()
    {
        if (_currentUser == null) { Console.WriteLine("Сначала войдите в систему."); return; }

        Console.Write("Введите ID версии модуля: "); // Предполагаем, что пользователь знает ID версии
                                                     // В идеале, сначала показать модуль, потом его версии для выбора
        if (!int.TryParse(Console.ReadLine(), out int moduleVersionId))
        {
            Console.WriteLine("Некорректный ID версии модуля."); return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var moduleVersion = context.ModuleVersions.Find(moduleVersionId);
            if (moduleVersion == null) { Console.WriteLine("Версия модуля не найдена."); return; }

            Console.Write("Модель вашего устройства: ");
            string deviceModel = Console.ReadLine();
            Console.Write("Версия Android: ");
            string androidVersion = Console.ReadLine();
            Console.Write("Статус работы (Works/WorksWithIssues/DoesNotWork): ");
            string worksStatus = Console.ReadLine();
            Console.Write("Дополнительные заметки (необязательно): ");
            string notes = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(deviceModel) || string.IsNullOrWhiteSpace(androidVersion) || string.IsNullOrWhiteSpace(worksStatus))
            {
                Console.WriteLine("Модель, версия Android и статус должны быть заполнены."); return;
            }

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
            try
            {
                context.CompatibilityReports.Add(report);
                context.SaveChanges();
                Console.WriteLine("Отчет о совместимости успешно добавлен!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении отчета: {ex.Message}");
            }
        }
    }


    // --- ДЕЙСТВИЯ ДЛЯ РАЗРАБОТЧИКА ---
    static void UploadNewModule()
    {
        if (_currentUser == null || _currentUser.Role.RoleName != "Developer")
        {
            Console.WriteLine("Действие доступно только для разработчиков."); return;
        }

        Console.Write("Название нового модуля: ");
        string name = Console.ReadLine();
        Console.Write("Описание модуля: ");
        string description = Console.ReadLine();

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            // Вывод списка категорий для выбора
            var categories = context.ModuleCategories.ToList();
            if (!categories.Any()) { Console.WriteLine("Категории не найдены. Сначала добавьте категории."); return; }
            Console.WriteLine("Доступные категории:");
            foreach (var cat in categories) { Console.WriteLine($"{cat.CategoryID}. {cat.CategoryName}"); }
            Console.Write("Выберите ID категории: ");
            if (!int.TryParse(Console.ReadLine(), out int categoryId) || !categories.Any(c => c.CategoryID == categoryId))
            {
                Console.WriteLine("Некорректный ID категории."); return;
            }

            // Для первого модуля также нужна первая версия
            Console.Write("Версия модуля (например, v1.0): ");
            string versionString = Console.ReadLine();
            Console.Write("Ссылка на скачивание файла модуля: ");
            string downloadLink = Console.ReadLine();
            Console.Write("Changelog (можно оставить пустым): ");
            string changelog = Console.ReadLine();
            Console.Write("Минимальная версия Magisk (FE: 28000): ");
            string minMagiskVersion = Console.ReadLine();


            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(versionString) || string.IsNullOrWhiteSpace(downloadLink) || string.IsNullOrWhiteSpace(minMagiskVersion))
            {
                Console.WriteLine("Название, описание, версия, версия Magisk и ссылка на скачивание обязательны."); return;
            }

            var newModule = new Module
            {
                Name = name,
                Description = description,
                AuthorUserID = _currentUser.UserID,
                CategoryID = categoryId,
                IsVerified = false, // Модули требуют верификации
                CreationDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow
            };

            var newVersion = new ModuleVersion
            {
                Module = newModule, // EF Core свяжет это при добавлении newModule
                VersionString = versionString,
                DownloadLink = downloadLink,
                Changelog = changelog,
                UploadDate = DateTime.UtcNow,
                MinMagiskVersion = minMagiskVersion
            };

            try
            {
                // context.Modules.Add(newModule); // Достаточно добавить модуль, версия добавится каскадно
                context.ModuleVersions.Add(newVersion); // Или так, если newModule уже существует или добавляется отдельно
                                                        // Лучше явно добавить обе сущности, если они новые и связаны
                context.Modules.Add(newModule);
                context.SaveChanges();
                Console.WriteLine($"Модуль '{name}' и его версия '{versionString}' успешно загружены и ожидают верификации.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке модуля: {ex.Message} {(ex.InnerException != null ? "Inner: " + ex.InnerException.Message : "")}");
            }
        }
    }

    static void AddVersionToMyModule()
    {
        if (_currentUser == null || _currentUser.Role.RoleName != "Developer")
        {
            Console.WriteLine("Действие доступно только для разработчиков."); return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var myModules = context.Modules
                                    .Where(m => m.AuthorUserID == _currentUser.UserID)
                                    .ToList();
            if (!myModules.Any()) { Console.WriteLine("У вас пока нет загруженных модулей."); return; }

            Console.WriteLine("Ваши модули:");
            foreach (var mod in myModules) { Console.WriteLine($"ID: {mod.ModuleID}, Название: {mod.Name}"); }
            Console.Write("Введите ID модуля для добавления версии: ");
            if (!int.TryParse(Console.ReadLine(), out int moduleId) || !myModules.Any(m => m.ModuleID == moduleId))
            {
                Console.WriteLine("Некорректный ID модуля или модуль вам не принадлежит."); return;
            }

            Console.Write("Новая версия (например, v1.1): ");
            string versionString = Console.ReadLine();
            Console.Write("Ссылка на скачивание: ");
            string downloadLink = Console.ReadLine();
            Console.Write("Changelog: ");
            string changelog = Console.ReadLine();
            Console.Write("Минимальная версия Magisk (FE: 28001) ");
            string minMagiskVersion = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(versionString) || string.IsNullOrWhiteSpace(downloadLink) || string.IsNullOrWhiteSpace(minMagiskVersion))
            {
                Console.WriteLine("Версия, версия Magisk и ссылка на скачивание обязательны."); return;
            }

            var newVersion = new ModuleVersion
            {
                ModuleID = moduleId,
                VersionString = versionString,
                DownloadLink = downloadLink,
                Changelog = changelog,
                UploadDate = DateTime.UtcNow,
                MinMagiskVersion = minMagiskVersion
            };

            try
            {
                context.ModuleVersions.Add(newVersion);
                // Обновим LastUpdateDate у модуля
                var moduleToUpdate = context.Modules.Find(moduleId);
                if (moduleToUpdate != null) moduleToUpdate.LastUpdateDate = DateTime.UtcNow;

                context.SaveChanges();
                Console.WriteLine($"Новая версия '{versionString}' для модуля ID {moduleId} успешно добавлена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении версии: {ex.Message}");
            }
        }
    }

    static void ViewMyModules()
    {
        if (_currentUser == null || _currentUser.Role.RoleName != "Developer")
        {
            Console.WriteLine("Действие доступно только для разработчиков."); return;
        }
        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var myModules = context.Modules
                               .Where(m => m.AuthorUserID == _currentUser.UserID)
                               .Include(m => m.Category)
                               .Include(m => m.Versions) // Подгружаем версии
                               .ToList();
            if (!myModules.Any()) { Console.WriteLine("У вас пока нет загруженных модулей."); return; }

            Console.WriteLine("\n--- Ваши модули ---");
            foreach (var mod in myModules)
            {
                Console.WriteLine($"ID: {mod.ModuleID}, Название: {mod.Name}, Категория: {mod.Category.CategoryName}, Верифицирован: {mod.IsVerified}");
                if (mod.Versions.Any())
                {
                    Console.WriteLine("  Версии:");
                    foreach (var ver in mod.Versions.OrderByDescending(v => v.UploadDate))
                    {
                        Console.WriteLine($"    - {ver.VersionString} (Загружено: {ver.UploadDate.ToShortDateString()})");
                    }
                }
                else
                {
                    Console.WriteLine("  (Версий нет)");
                }
            }
        }
    }

    // --- ДЕЙСТВИЯ ДЛЯ МОДЕРАТОРА ---
    static void VerifyModule()
    {
        if (_currentUser == null || _currentUser.Role.RoleName != "Moderator")
        {
            Console.WriteLine("Действие доступно только для модераторов."); return;
        }

        using (var context = new MagiskHubContext(_dbContextOptions))
        {
            var unverifiedModules = context.Modules
                                            .Where(m => !m.IsVerified)
                                            .Include(m => m.Author)
                                            .ToList();

            if (!unverifiedModules.Any())
            {
                Console.WriteLine("Нет модулей, ожидающих верификации."); return;
            }

            Console.WriteLine("\n--- Модули, ожидающие верификации ---");
            foreach (var mod in unverifiedModules)
            {
                Console.WriteLine($"ID: {mod.ModuleID}, Название: {mod.Name}, Автор: {mod.Author.Username}");
            }
            Console.Write("Введите ID модуля для верификации (или 0 для отмены): ");
            if (!int.TryParse(Console.ReadLine(), out int moduleId) || moduleId == 0) { return; }

            var moduleToVerify = context.Modules.Find(moduleId);
            if (moduleToVerify == null || moduleToVerify.IsVerified)
            {
                Console.WriteLine("Модуль не найден или уже верифицирован."); return;
            }

            Console.Write($"Верифицировать модуль '{moduleToVerify.Name}'? (yes/no): ");
            string confirmation = Console.ReadLine().ToLower();
            if (confirmation == "yes")
            {
                moduleToVerify.IsVerified = true;
                moduleToVerify.LastUpdateDate = DateTime.UtcNow; // Обновляем дату, т.к. было изменение
                try
                {
                    context.SaveChanges();
                    Console.WriteLine($"Модуль '{moduleToVerify.Name}' успешно верифицирован.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при верификации: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Верификация отменена.");
            }
        }
    }
}
