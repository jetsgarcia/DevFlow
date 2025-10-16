using DevFlow.Data;
using DevFlow.Models;
using DevFlow.Models.DTOs;
using DevFlow.Utilities;
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

        // Check if there's ANY active session across all projects (user can only work on one project at a time)
        var anyActiveSession = await _context.Sessions
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.IsActive);

        if (anyActiveSession != null)
        {
            // Check if it's for the same project
            if (anyActiveSession.ProjectId == startSessionDto.ProjectId)
            {
                _logger.LogWarning(
                    "Session start failed: Project {ProjectId} already has an active session (SessionId: {SessionId})",
                    startSessionDto.ProjectId, 
                    anyActiveSession.Id);
                throw new InvalidOperationException(
                    $"Project '{anyActiveSession.Project.Name}' already has an active session. Please stop the current session before starting a new one.");
            }
            else
            {
                _logger.LogWarning(
                    "Session start failed: Another project ({ProjectId}) already has an active session (SessionId: {SessionId}). Cannot start session for ProjectId: {RequestedProjectId}",
                    anyActiveSession.ProjectId,
                    anyActiveSession.Id,
                    startSessionDto.ProjectId);
                throw new InvalidOperationException(
                    $"You already have an active session on project '{anyActiveSession.Project.Name}'. You can only work on one project at a time. Please stop the current session before starting a new one.");
            }
        }

        // Verify that the project exists
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == startSessionDto.ProjectId);

        if (project == null)
        {
            _logger.LogWarning("Session start failed: Project with ID {ProjectId} not found", startSessionDto.ProjectId);
            throw new InvalidOperationException($"Project with ID {startSessionDto.ProjectId} not found.");
        }

        // Create new session entity
        var session = new Session
        {
            ProjectId = startSessionDto.ProjectId,
            StartTime = DateTimeHelper.Now,
            IsActive = true,
            IsAutoStopped = false,
            EndTime = null,
            DurationSeconds = null,
            CreatedAt = DateTimeHelper.Now
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
        session.EndTime = DateTimeHelper.Now;
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

    /// <inheritdoc />
    public async Task<List<SessionDto>> GetProjectSessionsAsync(int projectId)
    {
        _logger.LogInformation("Retrieving all sessions for ProjectId: {ProjectId}", projectId);

        // Verify that the project exists
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            _logger.LogWarning("Get sessions failed: Project with ID {ProjectId} not found", projectId);
            throw new InvalidOperationException($"Project with ID {projectId} not found.");
        }

        // Retrieve all sessions for the project, ordered by start time (most recent first)
        var sessions = await _context.Sessions
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        _logger.LogInformation(
            "Successfully retrieved {Count} session(s) for ProjectId: {ProjectId}",
            sessions.Count,
            projectId);

        // Map to DTOs
        return sessions.Select(s => MapToSessionDto(s, project.Name)).ToList();
    }

    /// <inheritdoc />
    public async Task<ActiveSessionDto?> GetActiveSessionAsync(int projectId)
    {
        _logger.LogInformation("Checking for active session on ProjectId: {ProjectId}", projectId);

        // Verify that the project exists
        var project = await _context.Projects
            .Include(p => p.Sessions.Where(s => s.IsActive))
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            _logger.LogWarning("Get active session failed: Project with ID {ProjectId} not found", projectId);
            throw new InvalidOperationException($"Project with ID {projectId} not found.");
        }

        // Get the active session if one exists
        var activeSession = project.Sessions.FirstOrDefault(s => s.IsActive);

        if (activeSession == null)
        {
            _logger.LogInformation("No active session found for ProjectId: {ProjectId}", projectId);
            return null;
        }

        _logger.LogInformation(
            "Active session found: SessionId={SessionId} for ProjectId={ProjectId}",
            activeSession.Id,
            projectId);

        // Map to ActiveSessionDto
        return MapToActiveSessionDto(activeSession, project.Name);
    }

    /// <inheritdoc />
    public async Task<ActiveSessionDto?> GetAnyActiveSessionAsync()
    {
        _logger.LogInformation("Checking for any active session across all projects");

        // Get any active session from any project
        var activeSession = await _context.Sessions
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.IsActive);

        if (activeSession == null)
        {
            _logger.LogInformation("No active session found across all projects");
            return null;
        }

        _logger.LogInformation(
            "Active session found: SessionId={SessionId} for ProjectId={ProjectId}",
            activeSession.Id,
            activeSession.ProjectId);

        // Map to ActiveSessionDto
        return MapToActiveSessionDto(activeSession, activeSession.Project.Name);
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

    /// <summary>
    /// Maps a Session entity to ActiveSessionDto
    /// </summary>
    private static ActiveSessionDto MapToActiveSessionDto(Session session, string projectName)
    {
        var elapsedSeconds = (int)(DateTimeHelper.Now - session.StartTime).TotalSeconds;

        return new ActiveSessionDto
        {
            Id = session.Id,
            ProjectId = session.ProjectId,
            ProjectName = projectName,
            StartTime = session.StartTime,
            ElapsedSeconds = elapsedSeconds
        };
    }
}
