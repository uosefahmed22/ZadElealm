using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.ComponentModel.DataAnnotations;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.Auth
{
    public class UpdateProfileCommand : BaseCommand<ApiResponse>
    {
        public string? DisplayName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }

        public UpdateProfileCommand( string? displayName, string? phoneNumber, string email)
        {
            Email = email;
            DisplayName = displayName;
            PhoneNumber = phoneNumber;
        }
    }
}
