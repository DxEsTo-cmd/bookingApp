using BookingApp.Data;
using BookingApp.Data.Models;
using BookingApp.DTOs.Resource;
using BookingApp.Repositories.Bases;
using BookingApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Repositories
{
    public class ResourcesRepository
        : ActEntityRepositoryBase<Resource, int, ApplicationUser, string>,
        IResourcesRepository
    {
        DbSet<Resource> Resources => Entities;

        public ResourcesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task UpdateAsync(Resource resource) => await UpdateSelectiveAsync<ResourceUpdateSubsetDto>(resource);

        public async Task<IEnumerable<Resource>> ListByRuleKeyAsync(int ruleId) => await Resources.Where(r => r.RuleId == ruleId).ToListAsync();

        public async Task<Resource> GetResourceWithImages(int resourceId)
        {
            var resource = await Resources.Where(x => x.Id == resourceId).Include(x => x.Image).Include(x => x.Coordinates).FirstOrDefaultAsync();

            return resource;
        }

        public async Task<IEnumerable<Resource>> GetResourcesWithImages()
        {
            var resource = await Resources.Include(x => x.Image).ToListAsync();

            return resource;
        }

        public async Task<IEnumerable<Resource>> GetActiveResourcesWithImages()
        {
            var resource = await Resources.Where(x => x.IsActive == true).Include(x => x.Image).ToListAsync();

            return resource;
        }

        public async Task<IEnumerable<Resource>> ListByFolderKeyAsync(int folderId) => await Resources.Where(r => r.FolderId == folderId).ToListAsync();

        public async Task DeleteAllRelatedCoordinates(int resourceId)
        {
            var resource = await dbContext.Resources
                .Where(x => x.Id == resourceId)
                .Include(x => x.Coordinates).FirstOrDefaultAsync();

            dbContext.Coordinates.RemoveRange(resource.Coordinates);

            await SaveAsync();
        }

        #region MethodsForStatisticsService

        public async Task<IEnumerable<Resource>> ListIncludingBookingsAndRules() => await Resources.Include(r => r.Bookings).Include(r => r.Rule).ToListAsync();

        public async Task<Resource> GetIncludingBookingsAndRules(int resourceID) => await Resources.Include(r => r.Bookings).Include(r => r.Rule).SingleOrDefaultAsync(r => r.Id == resourceID);

        #endregion
    }
}