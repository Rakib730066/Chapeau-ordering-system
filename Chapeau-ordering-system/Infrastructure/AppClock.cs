namespace Chapeau_ordering_system.Infrastructure
{
    public static class AppClock
    {
        public static int MinutesSince(DateTime referenceTime)
        {
            return (int)(DateTime.Now - referenceTime).TotalMinutes;
        }
    }
}
