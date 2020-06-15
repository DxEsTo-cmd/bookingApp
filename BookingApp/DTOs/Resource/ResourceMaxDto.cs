using BookingApp.Data.Models;
using System.Collections.Generic;

namespace BookingApp.DTOs.Resource
{
    /// <summary>
    /// Suitable for detailed read. Has ID.
    /// </summary>
    public class ResourceMaxDto : ResourceDetailedDto
    {
        public int Id { get; set; }

        public IEnumerable<string> Image { get; set; }

        public IEnumerable<Coordinate> Coordinates { get; set; }
    }
}
