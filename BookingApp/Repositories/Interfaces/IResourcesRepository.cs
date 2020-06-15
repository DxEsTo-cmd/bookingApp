using BookingApp.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingApp.Repositories.Interfaces
{
    /// <summary>
    /// Specific Resources Interface repository for the Booking App.
    /// </summary>
    public interface IResourcesRepository
        : IActEntityRepository<Resource, int, ApplicationUser, string>
    {
        /// <summary>
        /// Lists all resources adhering to the specified rule.
        /// </summary>
        Task<IEnumerable<Resource>> ListByRuleKeyAsync(int ruleId);

        /// <summary>
        /// Lists all resources having specified parent tree group.
        /// </summary>
        Task<IEnumerable<Resource>> ListByFolderKeyAsync(int folderId);

        /// <summary>
        /// Get resource with image
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        Task<Resource> GetResourceWithImages(int resourceId);

        Task<IEnumerable<Resource>> GetResourcesWithImages();

        Task DeleteAllRelatedCoordinates(int resourceId);

        Task<IEnumerable<Resource>> GetActiveResourcesWithImages();

        #region MethodsForStatisticsController

        Task<IEnumerable<Resource>> ListIncludingBookingsAndRules();

        Task<Resource> GetIncludingBookingsAndRules(int resourceID);

        #endregion
    }
}