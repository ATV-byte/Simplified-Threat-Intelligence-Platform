namespace Simplified_Threat_Intelligence_Platform.Shared
{
    public static class Epoch
    {
        public static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public static long DaysAgo(int days) => Now() - days * 24L * 3600L;
    }
}
