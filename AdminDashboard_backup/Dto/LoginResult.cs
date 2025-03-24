using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Dto
{
    public class LoginResult
    {
        public bool Succeeded { get; init; }
        public string ErrorMessage { get; init; }
        public AppUser User { get; init; }
    }
}
    