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

        // GET /api/projects - Get all projects
        group.MapGet("/", GetAllProjects)
            .WithName("GetAllProjects")
            .WithSummary("Get all projects")
            .WithDescription("Retrieves a list of all projects with their summary information including total sessions and hours.")
            .Produces<ApiResponse<IEnumerable<ProjectDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // POST /api/projects - Create a new project
        group.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .WithSummary("Create a new project")
            .WithDescription("Creates a new project with the specified name and optional description. Project names must be unique.")
            .Produces<ApiResponse<ProjectDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);

        // PUT /api/projects/{id} - Update an existing project
        group.MapPut("/{id}", UpdateProject)
            .WithName("UpdateProject")
            .WithSummary("Update an existing project")
            .WithDescription("Updates the name and/or description of an existing project. Project names must be unique.")
            .Produces<ApiResponse<ProjectDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
            .Produces<ApiResponse<object>>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Gets all projects with their summary information
    /// </summary>
    private static async Task<IResult> GetAllProjects(
        [FromServices] IProjectService projectService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            var projects = await projectService.GetAllProjectsAsync();

            logger.LogInformation("Retrieved {ProjectCount} projects successfully", projects.Count());

            return Results.Ok(ApiResponse<IEnumerable<ProjectDto>>.SuccessResponse(
                projects,
                "Projects retrieved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error retrieving projects");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while retrieving projects");
            return Results.Problem(
                detail: "An unexpected error occurred while retrieving projects.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
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

    /// <summary>
    /// Updates an existing project
    /// </summary>
    private static async Task<IResult> UpdateProject(
        [FromRoute] int id,
        [FromBody] UpdateProjectDto updateProjectDto,
        [FromServices] IProjectService projectService,
        [FromServices] ILogger<Program> logger)
    {
        try
        {
            // Validate the model
            if (string.IsNullOrWhiteSpace(updateProjectDto.Name))
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                    "Project name is required and cannot be empty."));
            }

            // Validate ID
            if (id <= 0)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid project ID."));
            }

            var project = await projectService.UpdateProjectAsync(id, updateProjectDto);

            logger.LogInformation("Project updated successfully: {ProjectId} - {ProjectName}",
                project.Id, project.Name);

            return Results.Ok(ApiResponse<ProjectDto>.SuccessResponse(
                project,
                "Project updated successfully."));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Project update failed - not found: {Message}", ex.Message);
            return Results.NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            logger.LogWarning("Project update conflict: {Message}", ex.Message);
            return Results.Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error updating project");
            return Results.BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while updating project");
            return Results.Problem(
                detail: "An unexpected error occurred while updating the project.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
