using MagiskHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magisk_DB.IDataAcess
{
    public interface IReviewRepository
    {
        Review GetById(int reviewId);
        IEnumerable<Review> GetReviewsForModule(int moduleId);
        void Add(Review review);
        // void Update(Review review);
        // void Delete(int reviewId);
    }
}
