using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.IDataAcess
{
    public interface ICompatibilityReportRepository
    {
        CompatibilityReport GetById(int reportId);
        IEnumerable<CompatibilityReport> GetReportsForModuleVersion(int moduleVersionId);
        void Add(CompatibilityReport report);
    }
}
