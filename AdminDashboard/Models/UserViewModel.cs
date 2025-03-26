using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Is Deleted")]
        public bool IsDeleted { get; set; }

        [Display(Name = "Is Confirmed")]
        public bool IsConfirmed { get; set; }

        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        public List<string> Roles { get; set; }
    }
}
