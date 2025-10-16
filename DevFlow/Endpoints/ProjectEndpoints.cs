using DevFlow.Models;
using DevFlow.Models.DTOs;
using DevFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevFlow.Endpoints;

/// <summary>
/// Extension methods for mapping project-related endpoints
/// </summary>
public static class ProjectEndpoints
{
    /// <summary>
    /// Maps all project-related endpoints
    /// </summary>
    public static void MapProjectEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/projects")
            .WithTags("Projects")
            .WithOpenApi();

        // POST /api/projects - Create a new project
        group.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .WithSummary("Create a new project")
            .WithDescription("Creates a new project with the specified name and optional description. Project names must be unique.")
            .Produces<ApiResponse<ProjectDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Creates a new project
    /// </summary>
    private static async Task<IResult> CreateProject(
        [FromBody] CreateProjectDto createProjectDto,
        [FromServices] IProjectService projectService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Validate the model
            if (string.IsNullOrWhiteSpace(createProjectDto.Name))
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                    "Project name is required and cannot be empty."));
            }

            var project = await projectService.CreateProjectAsync(createProjectDto);

            logger.LogInformation("Project created successfully: {ProjectId} - {ProjectName}",
                project.Id, project.Name);

            return Results.Created(
                $"/api/projects/{project.Id}",
                ApiResponse<ProjectDto>.SuccessResponse(
                    project,
                    "Project created successfully."));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            logger.LogWarning("Project creation conflict: {Message}", ex.Message);
            return Results.Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error creating project");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while creating project");
            return Results.Problem(
                detail: "An unexpected error occurred while creating the project.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
