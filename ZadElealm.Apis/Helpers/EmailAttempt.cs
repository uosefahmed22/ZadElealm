namespace ZadElealm.Apis.Helpers
{
    public class EmailAttempt
    {
        public DateTime LastAttempt { get; set; }
        public int AttemptCount { get; set; }
    }
}
