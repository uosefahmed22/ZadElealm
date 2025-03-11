using Microsoft.AspNetCore.Identity;
using ZadElealm.Core.Models;

namespace AdminDashboard.Dto
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public ICollection<UserNotification> UserNotifications { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<Certificate> Certificates { get; set; }
        public ICollection<Progress> Progresses { get; set; }
    }
}
