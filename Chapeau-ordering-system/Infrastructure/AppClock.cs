namespace Chapeau_ordering_system.Infrastructure
{
    public static class AppClock
    {
        // Set once when the application process starts.
        public static readonly DateTime StartedAt = DateTime.Now;

        // Returns minutes elapsed since the later of the two times —
        // so orders older than the current session never show stale minutes.
        public static int MinutesSince(DateTime referenceTime)
        {
            DateTime effectiveStart = referenceTime < StartedAt ? StartedAt : referenceTime;
            return (int)(DateTime.Now - effectiveStart).TotalMinutes;
        }
    }
}
