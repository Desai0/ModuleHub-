using Magisk_DB.DataAcess;
using Magisk_DB.IDataAcess;
using Magisk_DB.Services.Exceptions;
using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.Services.Classes
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IModuleVersionRepository _moduleVersionRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly ICompatibilityReportRepository _compatibilityReportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IModuleCategoryRepository _categoryRepository;
        private readonly ITagRepository _tagRepository; // Для проверки существования тегов, если они будут присваиваться при создании модуля
        private readonly IRoleRepository _roleRepository;

        public ModuleService(
            IModuleRepository moduleRepository,
            IModuleVersionRepository moduleVersionRepository,
            IReviewRepository reviewRepository,
            ICompatibilityReportRepository compatibilityReportRepository,
            IUserRepository userRepository,
            IModuleCategoryRepository categoryRepository,
            ITagRepository tagRepository, // Добавлена зависимость
            IRoleRepository roleRepository)
        {
            _moduleRepository = moduleRepository;
            _moduleVersionRepository = moduleVersionRepository;
            _reviewRepository = reviewRepository;
            _compatibilityReportRepository = compatibilityReportRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<Module>> GetAllModulesAsync()
        {
            try
            {
                // Предполагаем, что GetAllModulesAsync из репозитория включает Author и Category
                return _moduleRepository.GetAllModulesWithAuthorAndCategory();
            }
            catch (DataAccessException ex)
            {
                // Логирование ex
                throw new BusinessRuleException("Не удалось получить список модулей.", ex);
            }
        }

        public async Task<IEnumerable<Module>> SearchModulesByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // Можно вернуть пустой список или выбросить исключение, в зависимости от требований
                return Enumerable.Empty<Module>();
            }
            try
            {
                return _moduleRepository.SearchModulesByName(searchTerm);
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Ошибка при поиске модулей.", ex);
            }
        }

        public async Task<Module> GetModuleByIdAsync(int moduleId)
        {
            try
            {
                // Этот метод должен загружать все необходимые детали: версии, отзывы, теги и т.д.
                var module = _moduleRepository.GetModuleByIdWithDetails(moduleId);
                if (module == null)
                {
                    throw new NotFoundException($"Модуль с ID {moduleId} не найден.");
                }
                return module;
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException($"Ошибка при получении модуля ID {moduleId}.", ex);
            }
        }

        public async Task AddReviewAsync(int moduleId, int userId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                throw new BusinessRuleException("Оценка должна быть в диапазоне от 1 до 5.");
            }
            if (string.IsNullOrWhiteSpace(comment) && rating < 3) // Например, для низких оценок комментарий обязателен
            {
                // throw new BusinessRuleException("Для оценок ниже 3 комментарий обязателен.");
            }


            try
            {
                var module = _moduleRepository.GetModuleById(moduleId); // Простой GetById для проверки существования
                if (module == null)
                {
                    throw new NotFoundException($"Модуль с ID {moduleId} для добавления отзыва не найден.");
                }

                var user = _userRepository.GetUserById(userId);
                if (user == null)
                {
                    throw new NotFoundException($"Пользователь с ID {userId} не найден.");
                }

                var review = new Review
                {
                    ModuleID = moduleId,
                    UserID = userId,
                    Rating = rating,
                    CommentText = comment,
                    ReviewDate = DateTime.UtcNow,
                    IsEdited = false
                };
                _reviewRepository.Add(review);
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Ошибка при сохранении отзыва.", ex);
            }
        }

        public async Task AddCompatibilityReportAsync(int moduleVersionId, int userId, string deviceModel, string androidVersion, string worksStatus, string notes)
        {
            if (string.IsNullOrWhiteSpace(deviceModel)) throw new BusinessRuleException("Модель устройства обязательна.");
            if (string.IsNullOrWhiteSpace(androidVersion)) throw new BusinessRuleException("Версия Android обязательна.");
            if (string.IsNullOrWhiteSpace(worksStatus)) throw new BusinessRuleException("Статус работы обязателен.");
            // Можно добавить валидацию для worksStatus (чтобы было одно из допустимых значений)

            try
            {
                var moduleVersion = _moduleVersionRepository.GetById(moduleVersionId);
                if (moduleVersion == null)
                {
                    throw new NotFoundException($"Версия модуля с ID {moduleVersionId} не найдена.");
                }

                var user = _userRepository.GetUserById(userId);
                if (user == null)
                {
                    throw new NotFoundException($"Пользователь с ID {userId} не найден.");
                }

                var report = new CompatibilityReport
                {
                    ModuleVersionID = moduleVersionId,
                    UserID = userId,
                    DeviceModel = deviceModel,
                    AndroidVersion = androidVersion,
                    WorksStatus = worksStatus,
                    UserNotes = notes,
                    ReportDate = DateTime.UtcNow
                };
                _compatibilityReportRepository.Add(report);
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Ошибка при сохранении отчета о совместимости.", ex);
            }
        }

        public async Task<Module> UploadNewModuleAsync(int authorUserId, string name, string description, int categoryId,
                                            string initialVersionString, string initialDownloadLink, string initialChangelog,
                                            string minMagiskVersion) // Добавили minMagiskVersion
        {
            if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Название модуля обязательно.");
            if (string.IsNullOrWhiteSpace(description)) throw new BusinessRuleException("Описание модуля обязательно.");
            if (string.IsNullOrWhiteSpace(initialVersionString)) throw new BusinessRuleException("Начальная версия модуля обязательна.");
            if (string.IsNullOrWhiteSpace(initialDownloadLink)) throw new BusinessRuleException("Ссылка на скачивание для начальной версии обязательна.");
            if (string.IsNullOrWhiteSpace(minMagiskVersion)) throw new BusinessRuleException("Минимальная версия Magisk обязательна для версии модуля."); // Если поле NOT NULL

            try
            {
                var author = _userRepository.GetUserById(authorUserId);
                if (author == null)
                {
                    throw new NotFoundException($"Пользователь-автор с ID {authorUserId} не найден.");
                }
                // Убедимся, что роль загружена, если GetUserByIdAsync не делает этого по умолчанию
                if (author.Role == null && author.RoleID > 0) // Предполагаем, что RoleId есть
                {
                    // Это обходной путь. В идеале, GetUserByIdAsync должен включать роль.
                    // Если вы уверены, что _roleRepository доступен (например, через DI в сервис), то можно так.
                    // Иначе, нужно модифицировать IUserRepository.
                    // var role = await _roleRepository.GetRoleByIdAsync(author.RoleId); 
                    // author.Role = role;
                    // Пока просто выбросим исключение, если роль не загружена, чтобы выявить проблему в DAL
                    throw new BusinessRuleException("Роль пользователя-автора не загружена. Проверьте реализацию UserRepository.GetUserByIdAsync.");
                }


                if (author.Role.RoleName != "Developer")
                {
                    throw new BusinessRuleException("Только пользователи с ролью 'Developer' могут загружать модули.");
                }

                var category = _categoryRepository.GetById(categoryId);
                if (category == null)
                {
                    throw new NotFoundException($"Категория с ID {categoryId} не найдена.");
                }

                if (await _moduleRepository.GetModuleByNameAsync(name) != null)
                {
                    throw new BusinessRuleException($"Модуль с названием '{name}' уже существует. Выберите другое название.");
                }

                var newModule = new Module
                {
                    Name = name,
                    Description = description,
                    AuthorUserID = authorUserId,
                    CategoryID = categoryId,
                    IsVerified = false,
                    CreationDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow,
                    Versions = new List<ModuleVersion>() // Инициализируем коллекцию
                };

                var firstVersion = new ModuleVersion
                {
                    // ModuleId будет установлен EF Core автоматически, если Module и ModuleVersion добавляются вместе
                    // и ModuleVersion добавлена в коллекцию Module.Versions
                    VersionString = initialVersionString,
                    DownloadLink = initialDownloadLink,
                    Changelog = initialChangelog,
                    MinMagiskVersion = minMagiskVersion, // Используем переданное значение
                    FileSizeMB = null,
                    UploadDate = DateTime.UtcNow,
                    // Module = newModule; // Можно так, или через добавление в коллекцию
                };

                newModule.Versions.Add(firstVersion); // Ключевой момент для правильной связки при добавлении

                // Теперь передаем только newModule в репозиторий,
                // EF Core поймет, что нужно добавить и связанную ModuleVersion
                await _moduleRepository.AddModuleAsync(newModule); // Меняем вызов на AddModuleAsync

                // После SaveChanges в AddModuleAsync, newModule и firstVersion будут иметь свои ID
                // Навигационные свойства Author и Category для newModule
                newModule.Author = author;
                newModule.Category = category;
                // firstVersion.Module = newModule; // Уже связано через коллекцию

                return newModule;
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Ошибка при загрузке нового модуля.", ex);
            }
        }

        public async Task<ModuleVersion> AddVersionToModuleAsync(int moduleId, int authorUserId,
                                                        string versionString, string downloadLink, string changelog,
                                                        string minMagiskVersion) // Добавили minMagiskVersion
        {
            if (string.IsNullOrWhiteSpace(versionString)) throw new BusinessRuleException("Строка версии обязательна.");
            if (string.IsNullOrWhiteSpace(downloadLink)) throw new BusinessRuleException("Ссылка на скачивание обязательна.");
            if (string.IsNullOrWhiteSpace(minMagiskVersion)) throw new BusinessRuleException("Минимальная версия Magisk обязательна для версии модуля."); // Если поле NOT NULL

            try
            {
                var module = _moduleRepository.GetModuleById(moduleId);
                if (module == null)
                {
                    throw new NotFoundException($"Модуль с ID {moduleId} не найден.");
                }

                if (module.AuthorUserID != authorUserId)
                {
                    throw new BusinessRuleException("Вы не являетесь автором этого модуля и не можете добавлять к нему новые версии.");
                }

                // Проверка на уникальность версии для данного модуля (предполагаем, что метод ExistsAsync есть в IModuleVersionRepository)
                //if (await _moduleVersionRepository.ExistsAsync(moduleId, versionString))
                //{
                //    throw new BusinessRuleException($"Версия '{versionString}' для модуля '{module.Name}' уже существует.");
                //}

                var newVersion = new ModuleVersion
                {
                    ModuleID = moduleId,
                    VersionString = versionString,
                    DownloadLink = downloadLink,
                    Changelog = changelog,
                    MinMagiskVersion = minMagiskVersion, // Используем переданное значение
                    UploadDate = DateTime.UtcNow
                    // FileSizeMB можно установить позже или запросить
                };

                _moduleVersionRepository.Add(newVersion);

                // Обновить дату последнего изменения модуля
                module.LastUpdateDate = DateTime.UtcNow;
                _moduleRepository.UpdateModule(module);

                newVersion.Module = module; // Для возврата с навигационным свойством, если нужно
                return newVersion;
            }
            catch (DataAccessException ex)
            {
                // Логирование ex.ToString() может быть полезно для отладки полной ошибки
                throw new BusinessRuleException("Ошибка при добавлении новой версии модуля.", ex);
            }
        }

        public async Task<IEnumerable<Module>> GetModulesByAuthorAsync(int authorUserId)
        {
            try
            {
                var user = _userRepository.GetUserById(authorUserId);
                if (user == null)
                {
                    throw new NotFoundException($"Пользователь с ID {authorUserId} не найден.");
                }
                // Можно добавить проверку, что это разработчик, если требуется
                return _moduleRepository.GetModulesByAuthor(authorUserId);
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException($"Ошибка при получении модулей автора ID {authorUserId}.", ex);
            }
        }
    }
}
