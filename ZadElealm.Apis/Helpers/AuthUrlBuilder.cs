namespace ZadElealm.Apis.Helpers
{
    public static class AuthUrlBuilder
    {
        public static string BuildConfirmEmailUrl(string apiBaseUrl, string encodedUserId, string encodedToken)
        {
            var normalizedApiBaseUrl = (apiBaseUrl ?? string.Empty).TrimEnd('/');
            return $"{normalizedApiBaseUrl}/api/Account/confirm-email?userId={encodedUserId}&token={encodedToken}";
        }

        public static string BuildFrontendLoginUrl(string frontendBaseUrl)
        {
            var normalizedFrontendBaseUrl = (frontendBaseUrl ?? string.Empty).TrimEnd('/');
            return $"{normalizedFrontendBaseUrl}/login";
        }
    }
}
