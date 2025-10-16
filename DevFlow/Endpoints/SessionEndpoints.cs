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
            .WithDescription("Starts a new coding session associated with a specific project. Only one active session per project is allowed at a time.")
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
        catch (InvalidOperationException ex) when (ex.Message.Contains("already has an active session"))
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
}
