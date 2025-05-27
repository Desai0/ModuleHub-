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
    public class ReviewRepository : IReviewRepository
    {
        private readonly IDbContextFactory<MagiskHubContext> _contextFactory;
        public ReviewRepository(IDbContextFactory<MagiskHubContext> contextFactory) { _contextFactory = contextFactory; }

        public Review GetById(int reviewId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Reviews.AsNoTracking().FirstOrDefault(r => r.ReviewID == reviewId);
        }
        public IEnumerable<Review> GetReviewsForModule(int moduleId)
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Reviews
                .Where(r => r.ModuleID == moduleId)
                .Include(r => r.User) // Показать автора отзыва
                .AsNoTracking()
                .ToList();
        }
        public void Add(Review review)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                context.Reviews.Add(review);
                context.SaveChanges();
            }
            catch (DbUpdateException ex) { throw new DataAccessException("Ошибка добавления отзыва.", ex); }
        }
    }
}
