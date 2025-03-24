namespace AdminDashboard.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
