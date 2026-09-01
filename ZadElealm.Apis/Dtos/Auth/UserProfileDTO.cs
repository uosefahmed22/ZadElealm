namespace ZadElealm.Apis.Dtos.Auth
{
    public class UserProfileDTO
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
