using BookingApp.Data.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Data.Models
{
    public class Image : IIdentifiable<int>, ITrackable<ApplicationUser, string>
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public string ImagePath { get; set; }

        public bool IsPrimary { get; set; }

        #region Tracking
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string CreatedUserId { get; set; }
        public string UpdatedUserId { get; set; }
        public ApplicationUser Creator { get; set; }
        public ApplicationUser Updater { get; set; }
        #endregion
    }
}
