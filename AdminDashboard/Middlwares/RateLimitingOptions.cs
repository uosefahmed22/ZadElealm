namespace AdminDashboard.Middlwares
{
    public class RateLimitingOptions
    {
        public int MaxRequests { get; set; } = 60;
        public int IntervalSeconds { get; set; } = 60;
    }
}
