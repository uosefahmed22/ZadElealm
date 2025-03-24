using Microsoft.AspNetCore.Identity;

namespace AdminDashboard.Dto
{
    public class AddAdminResult
    {
        public bool Succeeded { get; init; }
        public string ErrorMessage { get; init; }
        public IEnumerable<IdentityError> Errors { get; init; }
    }
}
