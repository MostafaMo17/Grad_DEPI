using System;

namespace MediCare.Web.Services;

// The API sends all times in UTC, We show them in Cairo time
public static class CairoTime
{
    private static readonly TimeZoneInfo CairoZone = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Egypt Standard Time" : "Africa/Cairo");

    public static DateTime FromUtc(DateTime utc)
    {
        DateTime utcValue = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcValue, CairoZone);
    }

    // Example: 25 Nov 2026 23:30
    public static string Format(DateTime utc)
    {
        return FromUtc(utc).ToString("dd MMM yyyy HH:mm");
    }
}