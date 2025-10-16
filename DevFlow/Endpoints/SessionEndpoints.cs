using DevFlow.Models;
using DevFlow.Models.DTOs;
using DevFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevFlow.Endpoints;

/// <summary>
/// Extension methods for mapping session-related endpoints
/// </summary>
public static class SessionEndpoints
{
    /// <summary>
    /// Maps all session-related endpoints
    /// </summary>
    public static void MapSessionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sessions")
            .WithTags("Sessions")
            .WithOpenApi();

        // POST /api/sessions/start - Start a new coding session
        group.MapPost("/start", StartSession)
            .WithName("StartSession")
            .WithSummary("Start a new coding session")
            .WithDescription("Starts a new coding session associated with a specific project. Only one active session is allowed at a time across all projects. If another session is already active (even on a different project), a 409 Conflict will be returned.")
            .Produces<ApiResponse<SessionDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // POST /api/sessions/end - End an active coding session
        group.MapPost("/end", EndSession)
            .WithName("EndSession")
            .WithSummary("End an active coding session")
            .WithDescription("Ends an active coding session manually. Calculates the session duration and marks it as inactive.")
            .Produces<ApiResponse<SessionDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // GET /api/sessions/project/{projectId} - Get all sessions for a project
        group.MapGet("/project/{projectId:int}", GetProjectSessions)
            .WithName("GetProjectSessions")
            .WithSummary("Get all sessions for a project")
            .WithDescription("Retrieves all previous sessions for a specific project, ordered by start time (most recent first).")
            .Produces<ApiResponse<List<SessionDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // GET /api/sessions/active/{projectId} - Check for active session
        group.MapGet("/active/{projectId:int}", GetActiveSession)
            .WithName("GetActiveSession")
            .WithSummary("Check if a project has an active session")
            .WithDescription("Checks if a project currently has an active session and returns its details if found. Returns null if no active session exists. Useful for preventing duplicate session starts.")
            .Produces<ApiResponse<ActiveSessionDto?>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // GET /api/sessions/active - Get any active session across all projects
        group.MapGet("/active", GetAnyActiveSession)
            .WithName("GetAnyActiveSession")
            .WithSummary("Get the current active session across all projects")
            .WithDescription("Retrieves the currently active session if one exists, regardless of which project it belongs to. Since a user can only work on one project at a time, this returns the single active session or null.")
            .Produces<ApiResponse<ActiveSessionDto?>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // GET /api/sessions/average-duration - Get average session duration across all projects
        group.MapGet("/average-duration", GetAverageSessionDuration)
            .WithName("GetAverageSessionDuration")
            .WithSummary("Get average session duration across all projects")
            .WithDescription("Calculates and returns the average duration of all completed sessions regardless of project. Includes statistics such as total completed sessions and total hours worked.")
            .Produces<ApiResponse<AverageSessionDurationDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // GET /api/sessions/statistics - Get comprehensive session statistics
        group.MapGet("/statistics", GetSessionStatistics)
            .WithName("GetSessionStatistics")
            .WithSummary("Get comprehensive session statistics across all projects")
            .WithDescription("Retrieves detailed statistics for all sessions including averages, totals, longest/shortest sessions, active sessions count, date ranges, and number of projects tracked.")
            .Produces<ApiResponse<SessionStatisticsDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // GET /api/sessions/statistics/project/{projectId} - Get statistics for a specific project
        group.MapGet("/statistics/project/{projectId:int}", GetProjectStatistics)
            .WithName("GetProjectStatistics")
            .WithSummary("Get comprehensive session statistics for a specific project")
            .WithDescription("Retrieves detailed statistics for a specific project including averages, totals, longest/shortest sessions, active session status, and date ranges.")
            .Produces<ApiResponse<ProjectSessionStatisticsDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Starts a new coding session for a project
    /// </summary>
    private static async Task<IResult> StartSession(
        [FromBody] StartSessionDto startSessionDto,
        [FromServices] ISessionService sessionService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Validate the model
            if (startSessionDto.ProjectId <= 0)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid project ID. ProjectId must be a positive integer."));
            }

            var session = await sessionService.StartSessionAsync(startSessionDto);

            logger.LogInformation("Session started successfully: SessionId={SessionId}, ProjectId={ProjectId}",
                session.Id, session.ProjectId);

            return Results.Created(
                $"/api/sessions/{session.Id}",
                ApiResponse<SessionDto>.SuccessResponse(
                    session,
                    "Coding session started successfully."));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Session start failed - project not found: {Message}", ex.Message);
            return Results.NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already has an active session") || ex.Message.Contains("You already have an active session"))
        {
            logger.LogWarning("Session start failed - active session exists: {Message}", ex.Message);
            return Results.Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error starting session");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while starting session");
            return Results.Problem(
                detail: "An unexpected error occurred while starting the session.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Ends an active coding session manually
    /// </summary>
    private static async Task<IResult> EndSession(
        [FromBody] EndSessionDto endSessionDto,
        [FromServices] ISessionService sessionService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Validate the model
            if (endSessionDto.SessionId <= 0)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid session ID. SessionId must be a positive integer."));
            }

            var session = await sessionService.EndSessionAsync(endSessionDto);

            logger.LogInformation(
                "Session ended successfully: SessionId={SessionId}, Duration={Duration} seconds",
                session.Id,
                session.DurationSeconds);

            return Results.Ok(ApiResponse<SessionDto>.SuccessResponse(
                session,
                "Coding session ended successfully."));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Session end failed - session not found: {Message}", ex.Message);
            return Results.NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already been ended"))
        {
            logger.LogWarning("Session end failed - session already ended: {Message}", ex.Message);
            return Results.Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error ending session");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while ending session");
            return Results.Problem(
                detail: "An unexpected error occurred while ending the session.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Retrieves all sessions for a specific project
    /// </summary>
    private static async Task<IResult> GetProjectSessions(
        [FromRoute] int projectId,
        [FromServices] ISessionService sessionService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Validate the project ID
            if (projectId <= 0)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid project ID. ProjectId must be a positive integer."));
            }

            var sessions = await sessionService.GetProjectSessionsAsync(projectId);

            logger.LogInformation(
                "Successfully retrieved {Count} session(s) for ProjectId={ProjectId}",
                sessions.Count,
                projectId);

            return Results.Ok(ApiResponse<List<SessionDto>>.SuccessResponse(
                sessions,
                $"Retrieved {sessions.Count} session(s) for the project."));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Get sessions failed - project not found: {Message}", ex.Message);
            return Results.NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error retrieving project sessions");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while retrieving project sessions");
            return Results.Problem(
                detail: "An unexpected error occurred while retrieving project sessions.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Checks if a project has an active session
    /// </summary>
    private static async Task<IResult> GetActiveSession(
        [FromRoute] int projectId,
        [FromServices] ISessionService sessionService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Validate the project ID
            if (projectId <= 0)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid project ID. ProjectId must be a positive integer."));
            }

            var activeSession = await sessionService.GetActiveSessionAsync(projectId);

            if (activeSession == null)
            {
                logger.LogInformation("No active session found for ProjectId={ProjectId}", projectId);
                return Results.Ok(ApiResponse<ActiveSessionDto?>.SuccessResponse(
                    null,
                    "No active session found for this project."));
            }

            logger.LogInformation(
                "Active session found: SessionId={SessionId} for ProjectId={ProjectId}",
                activeSession.Id,
                projectId);

            return Results.Ok(ApiResponse<ActiveSessionDto?>.SuccessResponse(
                activeSession,
                "Active session found."));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Get active session failed - project not found: {Message}", ex.Message);
            return Results.NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error checking for active session");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while checking for active session");
            return Results.Problem(
                detail: "An unexpected error occurred while checking for active session.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets any active session across all projects
    /// </summary>
    private static async Task<IResult> GetAnyActiveSession(
        [FromServices] ISessionService sessionService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            var activeSession = await sessionService.GetAnyActiveSessionAsync();

            if (activeSession == null)
            {
                logger.LogInformation("No active session found across all projects");
                return Results.Ok(ApiResponse<ActiveSessionDto?>.SuccessResponse(
                    null,
                    "No active session found."));
            }

            logger.LogInformation(
                "Active session found: SessionId={SessionId} for ProjectId={ProjectId}",
                activeSession.Id,
                activeSession.ProjectId);

            return Results.Ok(ApiResponse<ActiveSessionDto?>.SuccessResponse(
                activeSession,
                "Active session found."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while checking for any active session");
            return Results.Problem(
                detail: "An unexpected error occurred while checking for active session.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets the average session duration across all projects
    /// </summary>
    private static async Task<IResult> GetAverageSessionDuration(
        [FromServices] ISessionService sessionService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            var averageDuration = await sessionService.GetAverageSessionDurationAsync();

            logger.LogInformation(
                "Successfully calculated average session duration: {AverageSeconds} seconds across {TotalSessions} sessions",
                averageDuration.AverageDurationSeconds,
                averageDuration.TotalCompletedSessions);

            var message = averageDuration.TotalCompletedSessions > 0
                ? $"Average session duration calculated from {averageDuration.TotalCompletedSessions} completed session(s)."
                : "No completed sessions found yet. Start tracking your sessions to see statistics.";

            return Results.Ok(ApiResponse<AverageSessionDurationDto>.SuccessResponse(
                averageDuration,
                message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while calculating average session duration");
            return Results.Problem(
                detail: "An unexpected error occurred while calculating average session duration.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets comprehensive session statistics across all projects
    /// </summary>
    private static async Task<IResult> GetSessionStatistics(
        [FromServices] ISessionService sessionService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            var statistics = await sessionService.GetSessionStatisticsAsync();

            logger.LogInformation(
                "Successfully retrieved session statistics: {TotalSessions} completed, {Average} seconds average, {Active} active",
                statistics.TotalCompletedSessions,
                statistics.AverageDurationSeconds,
                statistics.ActiveSessions);

            var message = statistics.TotalCompletedSessions > 0
                ? $"Statistics calculated from {statistics.TotalCompletedSessions} completed session(s) across {statistics.TotalProjectsWithSessions} project(s)."
                : "No sessions found yet. Start tracking your sessions to see statistics.";

            return Results.Ok(ApiResponse<SessionStatisticsDto>.SuccessResponse(
                statistics,
                message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while retrieving session statistics");
            return Results.Problem(
                detail: "An unexpected error occurred while retrieving session statistics.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets comprehensive session statistics for a specific project
    /// </summary>
    private static async Task<IResult> GetProjectStatistics(
        [FromRoute] int projectId,
        [FromServices] ISessionService sessionService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Validate the project ID
            if (projectId <= 0)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid project ID. ProjectId must be a positive integer."));
            }

            var statistics = await sessionService.GetProjectStatisticsAsync(projectId);

            logger.LogInformation(
                "Successfully retrieved statistics for ProjectId: {ProjectId} - {TotalSessions} completed sessions, {Average} seconds average",
                projectId,
                statistics.TotalCompletedSessions,
                statistics.AverageDurationSeconds);

            var message = statistics.TotalCompletedSessions > 0
                ? $"Statistics calculated from {statistics.TotalCompletedSessions} completed session(s) for project '{statistics.ProjectName}'."
                : $"No completed sessions found for project '{statistics.ProjectName}' yet.";

            return Results.Ok(ApiResponse<ProjectSessionStatisticsDto>.SuccessResponse(
                statistics,
                message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Get project statistics failed - project not found: {Message}", ex.Message);
            return Results.NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error retrieving project statistics");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while retrieving project statistics");
            return Results.Problem(
                detail: "An unexpected error occurred while retrieving project statistics.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
