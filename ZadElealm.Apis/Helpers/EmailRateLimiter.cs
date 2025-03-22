using System.Collections.Concurrent;

namespace ZadElealm.Apis.Helpers
{
    public class EmailRateLimiter
    {
        private static readonly ConcurrentDictionary<string, EmailAttempt> _attemptTracker = new();

        public (bool CanSend, TimeSpan? WaitTime) CanSendEmail(string email)
        {
            var now = DateTime.UtcNow;
            var attempt = _attemptTracker.GetOrAdd(email, _ => new EmailAttempt());

            TimeSpan waitTime = GetWaitingTime(attempt.AttemptCount);

            if (attempt.LastAttempt.Add(waitTime) > now)
            {
                var timeToWait = attempt.LastAttempt.Add(waitTime) - now;
                return (false, timeToWait);
            }

            return (true, null);
        }

        public void RecordAttempt(string email)
        {
            _attemptTracker.AddOrUpdate(
                email,
                _ => new EmailAttempt { LastAttempt = DateTime.UtcNow, AttemptCount = 1 },
                (_, existing) => new EmailAttempt
                {
                    LastAttempt = DateTime.UtcNow,
                    AttemptCount = existing.AttemptCount + 1
                }
            );
        }

        private TimeSpan GetWaitingTime(int attemptCount)
        {
            return attemptCount switch
            {
                0 => TimeSpan.Zero,
                1 => TimeSpan.FromMinutes(1), 
                2 => TimeSpan.FromMinutes(5), 
                3 => TimeSpan.FromHours(1),   
                _ => TimeSpan.FromDays(1)     
            };
        }
    }

}
