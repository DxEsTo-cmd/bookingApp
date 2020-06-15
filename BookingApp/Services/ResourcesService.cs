using BookingApp.Data.Models;
using BookingApp.Exceptions;
using BookingApp.Repositories;
using BookingApp.Repositories.Interfaces;
using BookingApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BookingApp.Services
{
    public class ResourcesService : IResourcesService
    {
        readonly IResourcesRepository resourcesRepo;
        readonly IBookingsRepository bookingsRepo;
        private readonly string fulldPathToFolderImage = "../BookingApp/ClientApp/src/assets/resources/Resources/";
        private readonly string shortPathToFilderImage = "/assets/resources/Resources/";

        public ResourcesService(IResourcesRepository resourcesRepo, IBookingsRepository bookingsRepo)
        {
            this.resourcesRepo = resourcesRepo;
            this.bookingsRepo = bookingsRepo;
        }

        public async Task Create(Resource resource) => await resourcesRepo.CreateAsync(resource);
        public async Task<IEnumerable<Resource>> GetList() => await resourcesRepo.GetListAsync();
        public async Task<Resource> Get(int resourceId) => await resourcesRepo.GetResourceWithImages(resourceId);
        public async Task Update(Resource resource) => await resourcesRepo.UpdateAsync(resource);
        public async Task Delete(int resourceId) => await resourcesRepo.DeleteAsync(resourceId);

        public async Task<IEnumerable<int>> ListKeys() => await resourcesRepo.ListKeysAsync();
               
        public async Task<bool> IsActive(int resourceId) => await resourcesRepo.IsActiveAsync(resourceId);
        public async Task<IEnumerable<Resource>> ListActive() => await resourcesRepo.ListActiveAsync();
        public async Task<IEnumerable<int>> ListActiveKeys() => await resourcesRepo.ListActiveKeysAsync();

        public async Task<IEnumerable<Resource>> ListByAssociatedUser(string userId) => await resourcesRepo.ListByAssociatedUser(userId);
        public async Task<IEnumerable<Resource>> ListByRuleKey(int ruleId) => await resourcesRepo.ListByRuleKeyAsync(ruleId);
        public async Task<IEnumerable<Resource>> ListByFolderKey(int folderId) => await resourcesRepo.ListByFolderKeyAsync(folderId);

        public async Task<string> SaveResourceImage(IFormFile file)
        {
            if (file.Length > 0)
            {
                var fullPath = Path.Combine(fulldPathToFolderImage, file.FileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return await Task.FromResult(Path.Combine(shortPathToFilderImage, file.FileName));
            }
            else
            {
                throw new Exception("File can't be created");
            }
        }

        #region Occupancy
        public virtual async Task<double?> OccupancyByResource(int resourceId) => await bookingsRepo.OccupancyByResourceAsync(resourceId);

        public async Task<Dictionary<int, double?>> GetOccupancies()
        {
            var idsList = await ListKeys();
            return await GetOccupanciesByIDs(idsList);
        }

        public async Task<Dictionary<int, double?>> GetActiveOccupancies()
        {
            var idsList = await ListActiveKeys();
            return await GetOccupanciesByIDs(idsList);
        }

        public async Task<Dictionary<int, double?>> GetOccupanciesByIDs(IEnumerable<int> idsList)
        {
            var map = new Dictionary<int, double?>();

            foreach (int resourceId in idsList)
            {
                map.Add(resourceId, null);

                try
                {
                    map[resourceId] = await OccupancyByResource(resourceId);
                }
                catch (Exception ex) when (ex is KeyNotFoundException || ex is FieldValueAbsurdException)
                {
                    //silently swallowing disjoint values
                }
            }
            return map;
        }
        #endregion

        public async Task<IEnumerable<Resource>> ListResourcesWithImage(bool isActive)
        {
            if (isActive)
                return await resourcesRepo.GetActiveResourcesWithImages();
            else
                return await resourcesRepo.GetResourcesWithImages();
        }

        public async Task DeleteAllRelatedCoordinates(int resourceId)
        {
            await resourcesRepo.DeleteAllRelatedCoordinates(resourceId);
        }
    }
}