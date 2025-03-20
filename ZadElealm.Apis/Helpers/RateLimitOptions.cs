namespace ZadElealm.Apis.Helpers
{
    public class RateLimitOptions
    {
        public int MaxRequests { get; set; }
        public int TimeWindowMinutes { get; set; }
    }
}
