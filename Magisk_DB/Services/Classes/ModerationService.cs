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
    public class ModerationService : IModerationService
    {
        private readonly IModuleCategoryRepository _categoryRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IModuleRepository _moduleRepository;

        public ModerationService(
            IModuleCategoryRepository categoryRepository,
            ITagRepository tagRepository,
            IModuleRepository moduleRepository)
        {
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _moduleRepository = moduleRepository;
        }

        // --- Управление Категориями ---

        public async Task<IEnumerable<ModuleCategory>> GetAllCategoriesAsync()
        {
            try
            {
                return _categoryRepository.GetAll();
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Не удалось получить список категорий.", ex);
            }
        }

        public async Task<ModuleCategory> AddCategoryAsync(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new BusinessRuleException("Название категории не может быть пустым.");
            }

            try
            {
                if (_categoryRepository.GetByName(name) != null)
                {
                    throw new BusinessRuleException($"Категория с названием '{name}' уже существует.");
                }

                var category = new ModuleCategory
                {
                    CategoryName = name,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description
                };
                _categoryRepository.Add(category);
                return category; // category теперь должен иметь ID
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Ошибка при добавлении категории.", ex);
            }
        }

        public async Task<ModuleCategory> UpdateCategoryAsync(int categoryId, string newName, string newDescription)
        {
            try
            {
                var category = _categoryRepository.GetById(categoryId);
                if (category == null)
                {
                    throw new NotFoundException($"Категория с ID {categoryId} не найдена.");
                }

                bool hasChanges = false;

                if (!string.IsNullOrWhiteSpace(newName) && !category.CategoryName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                {
                    // Проверяем, не занято ли новое имя другой категорией
                    var existingCategoryWithNewName = _categoryRepository.GetByName(newName);
                    if (existingCategoryWithNewName != null && existingCategoryWithNewName.CategoryID != categoryId)
                    {
                        throw new BusinessRuleException($"Категория с названием '{newName}' уже существует.");
                    }
                    category.CategoryName = newName;
                    hasChanges = true;
                }

                // Сравниваем описания, учитывая null
                var currentDescription = category.Description ?? "";
                var updatedDescription = string.IsNullOrWhiteSpace(newDescription) ? "" : newDescription;

                if (!currentDescription.Equals(updatedDescription, StringComparison.Ordinal))
                {
                    category.Description = string.IsNullOrWhiteSpace(newDescription) ? null : newDescription;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    _categoryRepository.Update(category);
                }
                return category;
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException($"Ошибка при обновлении категории ID {categoryId}.", ex);
            }
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            try
            {
                var category = _categoryRepository.GetById(categoryId); // Для получения имени для сообщения
                if (category == null)
                {
                    throw new NotFoundException($"Категория с ID {categoryId} не найдена.");
                }

                if (_categoryRepository.IsCategoryInUse(categoryId))
                {
                    throw new BusinessRuleException($"Нельзя удалить категорию '{category.CategoryName}', так как она используется одним или несколькими модулями. Сначала измените категорию у этих модулей.");
                }

                bool deleted = _categoryRepository.Delete(categoryId);
                if (!deleted)
                {
                    // Это может произойти, если категория была удалена между GetByIdAsync и DeleteAsync,
                    // или если DeleteAsync имеет дополнительную логику, которая вернула false.
                    throw new DataAccessException($"Не удалось удалить категорию ID {categoryId} из базы данных, возможно, она уже была удалена или возникла другая проблема.");
                }
            }
            catch (DataAccessException ex)
            {
                // Если DataAccessException уже был выброшен из репозитория (например, при нарушении FK, если IsCategoryInUseAsync не сработал идеально)
                throw new BusinessRuleException($"Ошибка при удалении категории ID {categoryId}.", ex);
            }
        }


        // --- Управление Тегами ---

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            try
            {
                return _tagRepository.GetAll();
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Не удалось получить список тегов.", ex);
            }
        }

        public async Task<Tag> AddTagAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new BusinessRuleException("Название тега не может быть пустым.");
            }

            try
            {
                if (_tagRepository.GetByName(name) != null)
                {
                    throw new BusinessRuleException($"Тег с названием '{name}' уже существует.");
                }

                var tag = new Tag { TagName = name };
                _tagRepository.Add(tag);
                return tag; // tag теперь должен иметь ID
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Ошибка при добавлении тега.", ex);
            }
        }

        public async Task<Tag> UpdateTagAsync(int tagId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new BusinessRuleException("Новое название тега не может быть пустым.");
            }

            try
            {
                var tag = _tagRepository.GetById(tagId);
                if (tag == null)
                {
                    throw new NotFoundException($"Тег с ID {tagId} не найден.");
                }

                if (tag.TagName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                {
                    return tag; // Нет изменений
                }

                var existingTagWithNewName = _tagRepository.GetByName(newName);
                if (existingTagWithNewName != null && existingTagWithNewName.TagID != tagId)
                {
                    throw new BusinessRuleException($"Тег с названием '{newName}' уже существует.");
                }

                tag.TagName = newName;
                _tagRepository.Update(tag);
                return tag;
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException($"Ошибка при обновлении тега ID {tagId}.", ex);
            }
        }

        public async Task DeleteTagAsync(int tagId)
        {
            try
            {
                var tag = _tagRepository.GetById(tagId); // Для получения имени для сообщения
                if (tag == null)
                {
                    throw new NotFoundException($"Тег с ID {tagId} не найден.");
                }

                if ( _tagRepository.IsTagInUse(tagId))
                {
                    throw new BusinessRuleException($"Нельзя удалить тег '{tag.TagName}', так как он используется одним или несколькими модулями. Сначала удалите этот тег у соответствующих модулей.");
                }

                bool deleted = _tagRepository.Delete(tagId);
                if (!deleted)
                {
                    throw new DataAccessException($"Не удалось удалить тег ID {tagId} из базы данных.");
                }
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException($"Ошибка при удалении тега ID {tagId}.", ex);
            }
        }

        public async Task AssignTagsToModuleAsync(int moduleId, IEnumerable<int> newTagIds)
        {
            if (newTagIds == null) newTagIds = Enumerable.Empty<int>(); // Обработка null

            try
            {
                var module = _moduleRepository.GetModuleById(moduleId); // Простой GetById для проверки существования
                if (module == null)
                {
                    throw new NotFoundException($"Модуль с ID {moduleId} не найден.");
                }

                // Проверка, что все переданные теги существуют
                foreach (var tagId in newTagIds.Distinct()) // Distinct на случай дубликатов во входных данных
                {
                    if (_tagRepository.GetById(tagId) == null)
                    {
                        throw new NotFoundException($"Тег с ID {tagId} не найден и не может быть присвоен модулю.");
                    }
                }

                //var currentTagIds = (await _moduleRepository.GetTagIdsForModule(moduleId)).ToList();
                //var distinctNewTagIds = newTagIds.Distinct().ToList();


                //var tagIdsToAdd = distinctNewTagIds.Except(currentTagIds).ToList();
                //var tagIdsToRemove = currentTagIds.Except(distinctNewTagIds).ToList();

                //if (tagIdsToRemove.Any())
                //{
                //    _moduleRepository.RemoveTagFromModule(moduleId, tagIdsToRemove);
                //}

                //if (tagIdsToAdd.Any())
                //{
                //    _moduleRepository.AddTagToModule(moduleId, tagIdsToAdd);
                //}
                // Можно добавить обновление LastUpdateDate для модуля, если присвоение тегов считается изменением
                // module.LastUpdateDate = DateTime.UtcNow;
                // await _moduleRepository.UpdateModuleAsync(module);
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException($"Ошибка при присвоении тегов модулю ID {moduleId}.", ex);
            }
        }


        // --- Верификация Модулей ---

        public async Task<IEnumerable<Module>> GetUnverifiedModulesAsync()
        {
            try
            {
                // Предполагаем, что репозиторий включает Author для отображения
                return _moduleRepository.GetUnverifiedModules();
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException("Не удалось получить список не верифицированных модулей.", ex);
            }
        }

        public async Task<Module> VerifyModuleAsync(int moduleId, bool isVerifiedStatus)
        {
            try
            {
                var module = _moduleRepository.GetModuleById(moduleId); // Простой GetById
                if (module == null)
                {
                    throw new NotFoundException($"Модуль с ID {moduleId} не найден.");
                }

                if (module.IsVerified == isVerifiedStatus)
                {
                    // Нет изменений, просто возвращаем текущее состояние
                    // Можно добавить загрузку Author и Category, если они нужны для возврата
                    // var fullModule = await _moduleRepository.GetModuleByIdWithDetailsAsync(moduleId);
                    // return fullModule ?? module; // Если вдруг GetModuleByIdWithDetailsAsync вернул null после этого
                    return module;
                }

                module.IsVerified = isVerifiedStatus;
                module.LastUpdateDate = DateTime.UtcNow; // Важно обновить дату изменения

                _moduleRepository.UpdateModule(module);
                // Можно снова загрузить модуль с деталями, если нужно вернуть полную информацию
                // return await _moduleRepository.GetModuleByIdWithDetailsAsync(moduleId);
                return module; // Возвращаем обновленный модуль (может не содержать всех связей)
            }
            catch (DataAccessException ex)
            {
                throw new BusinessRuleException($"Ошибка при изменении статуса верификации модуля ID {moduleId}.", ex);
            }
        }
    }
}
