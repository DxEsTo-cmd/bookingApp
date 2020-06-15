using BookingApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.DTOs.Resource
{
    public class ResourceMarkerDto
    {
        public int Id { get; set; }

        public IList<Coordinate> Coordinates { get; set; }
    }
}
