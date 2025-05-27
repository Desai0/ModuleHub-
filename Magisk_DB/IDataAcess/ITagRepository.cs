using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.IDataAcess
{
    public interface ITagRepository
    {
        Tag GetById(int tagId);
        Tag GetByName(string tagName);
        IEnumerable<Tag> GetAll();
        void Add(Tag tag);
        void Update(Tag tag);
        bool Delete(int tagId);
        bool IsTagInUse(int tagId);
    }
}
