namespace AdminDashboard.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsDeleted { get; set; }
        public List<RoleViewModel> Roles { get; set; }
    }
}
