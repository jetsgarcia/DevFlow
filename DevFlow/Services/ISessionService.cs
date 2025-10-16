using DevFlow.Models.DTOs;

namespace DevFlow.Services;

/// <summary>
/// Interface defining session-related operations
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Starts a new coding session for a project
    /// </summary>
    /// <param name="startSessionDto">Session start details containing the project ID</param>
    /// <returns>The created session details</returns>
    /// <exception cref="InvalidOperationException">Thrown when the project is not found or already has an active session</exception>
    Task<SessionDto> StartSessionAsync(StartSessionDto startSessionDto);

    /// <summary>
    /// Ends an active coding session manually
    /// </summary>
    /// <param name="endSessionDto">Session end details containing the session ID</param>
    /// <returns>The updated session details</returns>
    /// <exception cref="InvalidOperationException">Thrown when the session is not found or already ended</exception>
    Task<SessionDto> EndSessionAsync(EndSessionDto endSessionDto);

    /// <summary>
    /// Retrieves all sessions for a specific project, ordered by start time (most recent first)
    /// </summary>
    /// <param name="projectId">The ID of the project to retrieve sessions for</param>
    /// <returns>A list of all sessions associated with the project</returns>
    /// <exception cref="InvalidOperationException">Thrown when the project is not found</exception>
    Task<List<SessionDto>> GetProjectSessionsAsync(int projectId);

    /// <summary>
    /// Checks if a project has an active session and retrieves its details
    /// </summary>
    /// <param name="projectId">The ID of the project to check for active sessions</param>
    /// <returns>The active session details if one exists, otherwise null</returns>
    /// <exception cref="InvalidOperationException">Thrown when the project is not found</exception>
    Task<ActiveSessionDto?> GetActiveSessionAsync(int projectId);

    /// <summary>
    /// Retrieves the currently active session across all projects (if any exists)
    /// </summary>
    /// <returns>The active session details if one exists, otherwise null</returns>
    Task<ActiveSessionDto?> GetAnyActiveSessionAsync();

    /// <summary>
    /// Calculates the average duration of all completed sessions across all projects
    /// </summary>
    /// <returns>Average session duration statistics including total completed sessions</returns>
    Task<AverageSessionDurationDto> GetAverageSessionDurationAsync();

    /// <summary>
    /// Retrieves comprehensive session statistics across all projects
    /// </summary>
    /// <returns>Detailed statistics including averages, totals, longest/shortest sessions, and more</returns>
    Task<SessionStatisticsDto> GetSessionStatisticsAsync();

    /// <summary>
    /// Retrieves comprehensive session statistics for a specific project
    /// </summary>
    /// <param name="projectId">The ID of the project to retrieve statistics for</param>
    /// <returns>Detailed statistics for the project including averages, totals, longest/shortest sessions, and more</returns>
    /// <exception cref="InvalidOperationException">Thrown when the project is not found</exception>
    Task<ProjectSessionStatisticsDto> GetProjectStatisticsAsync(int projectId);
}
