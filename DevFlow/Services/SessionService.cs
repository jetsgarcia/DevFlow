using DevFlow.Data;
using DevFlow.Models;
using DevFlow.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DevFlow.Services;

/// <summary>
/// Service handling session-related business logic
/// </summary>
public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ApplicationDbContext context, ILogger<SessionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SessionDto> StartSessionAsync(StartSessionDto startSessionDto)
    {
        if (startSessionDto == null)
        {
            throw new ArgumentNullException(nameof(startSessionDto));
        }

        _logger.LogInformation("Starting new session for ProjectId: {ProjectId}", startSessionDto.ProjectId);

        // Verify that the project exists
        var project = await _context.Projects
            .Include(p => p.Sessions.Where(s => s.IsActive))
            .FirstOrDefaultAsync(p => p.Id == startSessionDto.ProjectId);

        if (project == null)
        {
            _logger.LogWarning("Session start failed: Project with ID {ProjectId} not found", startSessionDto.ProjectId);
            throw new InvalidOperationException($"Project with ID {startSessionDto.ProjectId} not found.");
        }

        // Check if there's already an active session for this project
        var activeSession = project.Sessions.FirstOrDefault(s => s.IsActive);
        if (activeSession != null)
        {
            _logger.LogWarning("Session start failed: Project {ProjectId} already has an active session (SessionId: {SessionId})", 
                startSessionDto.ProjectId, activeSession.Id);
            throw new InvalidOperationException(
                $"Project '{project.Name}' already has an active session. Please stop the current session before starting a new one.");
        }

        // Create new session entity
        var session = new Session
        {
            ProjectId = startSessionDto.ProjectId,
            StartTime = DateTime.UtcNow,
            IsActive = true,
            IsAutoStopped = false,
            EndTime = null,
            DurationSeconds = null,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully started session with ID: {SessionId} for project: {ProjectId}", 
                session.Id, startSessionDto.ProjectId);

            // Map to DTO
            return MapToSessionDto(session, project.Name);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while starting session for ProjectId: {ProjectId}", 
                startSessionDto.ProjectId);
            throw new InvalidOperationException("An error occurred while starting the session. Please try again.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<SessionDto> EndSessionAsync(EndSessionDto endSessionDto)
    {
        if (endSessionDto == null)
        {
            throw new ArgumentNullException(nameof(endSessionDto));
        }

        _logger.LogInformation("Ending session with SessionId: {SessionId}", endSessionDto.SessionId);

        // Retrieve the session with project information
        var session = await _context.Sessions
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.Id == endSessionDto.SessionId);

        if (session == null)
        {
            _logger.LogWarning("Session end failed: Session with ID {SessionId} not found", endSessionDto.SessionId);
            throw new InvalidOperationException($"Session with ID {endSessionDto.SessionId} not found.");
        }

        // Check if the session is already ended
        if (!session.IsActive)
        {
            _logger.LogWarning("Session end failed: Session {SessionId} is already ended", endSessionDto.SessionId);
            throw new InvalidOperationException($"Session with ID {endSessionDto.SessionId} has already been ended.");
        }

        // End the session
        session.EndTime = DateTime.UtcNow;
        session.IsActive = false;
        session.DurationSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;
        session.IsAutoStopped = false; // Manually stopped

        try
        {
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully ended session with ID: {SessionId}. Duration: {Duration} seconds",
                session.Id,
                session.DurationSeconds);

            // Map to DTO
            return MapToSessionDto(session, session.Project.Name);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while ending session with SessionId: {SessionId}",
                endSessionDto.SessionId);
            throw new InvalidOperationException("An error occurred while ending the session. Please try again.", ex);
        }
    }

    /// <summary>
    /// Maps a Session entity to SessionDto
    /// </summary>
    private static SessionDto MapToSessionDto(Session session, string projectName)
    {
        return new SessionDto
        {
            Id = session.Id,
            ProjectId = session.ProjectId,
            ProjectName = projectName,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            DurationSeconds = session.DurationSeconds,
            IsActive = session.IsActive,
            IsAutoStopped = session.IsAutoStopped,
            CreatedAt = session.CreatedAt
        };
    }
}
