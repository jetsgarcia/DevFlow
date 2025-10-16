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
}
