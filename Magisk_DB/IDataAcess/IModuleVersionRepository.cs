using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.IDataAcess
{
    public interface IModuleVersionRepository
    {
        ModuleVersion GetById(int versionId);
        void Add(ModuleVersion version);
    }
}
