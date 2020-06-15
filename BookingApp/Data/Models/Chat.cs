using BookingApp.Data.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Data.Models
{
    public class Chat : IIdentifiable<int>, ITrackable<ApplicationUser, string>
    {
        public int Id { get; set; }
        public IList<ChatMessage> Messages { get; set; }
        public Resource Resource { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string CreatedUserId { get; set; }
        public string UpdatedUserId { get; set; }
        public ApplicationUser Creator { get; set; }
        public ApplicationUser Updater { get; set; }
    }
}
