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
    public class TagRepository : ITagRepository
    {
        private readonly IDbContextFactory<MagiskHubContext> _contextFactory;
        public TagRepository(IDbContextFactory<MagiskHubContext> contextFactory) { _contextFactory = contextFactory; }

        public Tag GetById(int tagId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Tags.AsNoTracking().FirstOrDefault(t => t.TagID == tagId);
        }
        public Tag GetByName(string tagName)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Tags.AsNoTracking().FirstOrDefault(t => t.TagName.ToLower() == tagName.ToLower());
        }
        public IEnumerable<Tag> GetAll()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Tags.AsNoTracking().ToList();
        }
        public void Add(Tag tag)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                context.Tags.Add(tag);
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка добавления тега '{tag.TagName}'.", ex); }
        }
        public void Update(Tag tag)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                context.Tags.Update(tag);
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка обновления тега '{tag.TagName}'.", ex); }
        }
        public bool Delete(int tagId)
        {
            using var context = _contextFactory.CreateDbContext();
            var tag = context.Tags.Find(tagId);
            if (tag == null) return false;
            try
            {
                context.Tags.Remove(tag);
                context.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex) { throw new DataAccessException($"Ошибка удаления тега ID {tagId}. Возможно, он используется.", ex); }
        }
        public bool IsTagInUse(int tagId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.ModuleTags.Any(mt => mt.TagID == tagId);
        }
    }
}
