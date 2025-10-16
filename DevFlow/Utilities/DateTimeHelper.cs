namespace DevFlow.Utilities;

/// <summary>
/// Helper class for handling timezone-specific DateTime operations
/// </summary>
public static class DateTimeHelper
{
    private static readonly TimeZoneInfo PhilippineTimeZone = 
        TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"); // UTC+8, same as Philippine Time

    /// <summary>
    /// Gets the current date and time in Philippine Time Zone (UTC+8)
    /// </summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhilippineTimeZone);
}
