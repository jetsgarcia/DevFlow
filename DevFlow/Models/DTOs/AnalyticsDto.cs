namespace DevFlow.Models.DTOs;

/// <summary>
/// DTO for overall flow analytics
/// </summary>
public class FlowAnalyticsDto
{
    public double AverageSessionDurationMinutes { get; set; }
    public double TotalHours { get; set; }
    public int TotalSessions { get; set; }
    public DateTime? FirstSessionDate { get; set; }
    public DateTime? LastSessionDate { get; set; }
}

/// <summary>
/// DTO for time range analytics request
/// </summary>
public class TimeRangeAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double TotalHours { get; set; }
    public int TotalSessions { get; set; }
    public double AverageSessionDurationMinutes { get; set; }
}

/// <summary>
/// DTO for project-specific analytics
/// </summary>
public class ProjectAnalyticsDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public double TotalHours { get; set; }
    public double AverageSessionDurationMinutes { get; set; }
    public DateTime? FirstSessionDate { get; set; }
    public DateTime? LastSessionDate { get; set; }
    public double? LongestSessionHours { get; set; }
    public double? ShortestSessionHours { get; set; }
}

/// <summary>
/// DTO for daily analytics
/// </summary>
public class DailyAnalyticsDto
{
    public DateTime Date { get; set; }
    public double TotalHours { get; set; }
    public int TotalSessions { get; set; }
}

/// <summary>
/// DTO for weekly analytics
/// </summary>
public class WeeklyAnalyticsDto
{
    public int Year { get; set; }
    public int WeekNumber { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public double TotalHours { get; set; }
    public int TotalSessions { get; set; }
}

/// <summary>
/// Query parameters for time range analytics
/// </summary>
public class TimeRangeQueryDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Enum for predefined time ranges
/// </summary>
public enum TimeRangeType
{
    Today,
    Yesterday,
    ThisWeek,
    LastWeek,
    ThisMonth,
    LastMonth,
    Custom
}

/// <summary>
/// DTO for average session duration across all projects
/// </summary>
public class AverageSessionDurationDto
{
    /// <summary>
    /// Average session duration in seconds across all projects (no decimal)
    /// </summary>
    public int AverageDurationSeconds { get; set; }

    /// <summary>
    /// Total number of completed sessions included in the calculation
    /// </summary>
    public int TotalCompletedSessions { get; set; }
}

/// <summary>
/// DTO for comprehensive session statistics across all projects
/// </summary>
public class SessionStatisticsDto
{
    /// <summary>
    /// Average session duration in seconds across all projects (no decimal)
    /// </summary>
    public int AverageDurationSeconds { get; set; }

    /// <summary>
    /// Total number of completed sessions
    /// </summary>
    public int TotalCompletedSessions { get; set; }

    /// <summary>
    /// Total duration of all completed sessions in seconds
    /// </summary>
    public int TotalSeconds { get; set; }

    /// <summary>
    /// Longest session duration in seconds
    /// </summary>
    public int? LongestSessionSeconds { get; set; }

    /// <summary>
    /// Shortest session duration in seconds
    /// </summary>
    public int? ShortestSessionSeconds { get; set; }

    /// <summary>
    /// Total number of active sessions (should be 0 or 1)
    /// </summary>
    public int ActiveSessions { get; set; }

    /// <summary>
    /// Date of the first session ever recorded
    /// </summary>
    public DateTime? FirstSessionDate { get; set; }

    /// <summary>
    /// Date of the most recent session
    /// </summary>
    public DateTime? LastSessionDate { get; set; }

    /// <summary>
    /// Number of unique projects with sessions
    /// </summary>
    public int TotalProjectsWithSessions { get; set; }
}
