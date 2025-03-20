namespace ZadElealm.Apis.Middlwares
{
    public class RateLimitOptions
    {
        public int MaxRequests { get; set; }
        public int TimeWindowMinutes { get; set; }
    }
}
