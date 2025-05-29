using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.Services
{
    public interface IModuleService
    {
        Task<IEnumerable<Module>> GetAllModulesAsync();
        Task<IEnumerable<Module>> SearchModulesByNameAsync(string searchTerm);
        Task<Module> GetModuleByIdAsync(int moduleId); // Для детальной информации, включая версии и отзывы
        Task AddReviewAsync(int moduleId, int userId, int rating, string comment);
        Task AddCompatibilityReportAsync(int moduleVersionId, int userId, string deviceModel, string androidVersion, string worksStatus, string notes);
        Task<Module> UploadNewModuleAsync(int authorUserId, string name, string description, int categoryId, string initialVersionString, string initialDownloadLink, string initialChangelog, string minMagiskVersion);
        Task<ModuleVersion> AddVersionToModuleAsync(int moduleId, int authorUserId,
                                                string versionString, string downloadLink, string changelog,
                                                string minMagiskVersion); // Добавили minMagiskVersion
        Task<IEnumerable<Module>> GetModulesByAuthorAsync(int authorUserId);

    }
}
