using Magisk_DB.IDataAcess;
using MagiskHub.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.DataAcess
{
    public class CompatibilityReportRepository : ICompatibilityReportRepository
    {
        private readonly IDbContextFactory<MagiskHubContext> _contextFactory;
        public CompatibilityReportRepository(IDbContextFactory<MagiskHubContext> contextFactory) { _contextFactory = contextFactory; }

        public CompatibilityReport GetById(int reportId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.CompatibilityReports.AsNoTracking().FirstOrDefault(cr => cr.ReportID == reportId);
        }
        public IEnumerable<CompatibilityReport> GetReportsForModuleVersion(int moduleVersionId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.CompatibilityReports
                .Where(cr => cr.ModuleVersionID == moduleVersionId)
                .Include(cr => cr.User) // Показать автора отчета
                .AsNoTracking()
                .ToList();
        }
        public void Add(CompatibilityReport report)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                context.CompatibilityReports.Add(report);
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException("Ошибка добавления отчета о совместимости.", ex); }
        }
    }
}
