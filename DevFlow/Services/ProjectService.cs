using DevFlow.Data;
using DevFlow.Models;
using DevFlow.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DevFlow.Services;

/// <summary>
/// Service handling project-related business logic
/// </summary>
public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ApplicationDbContext context, ILogger<ProjectService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto)
    {
        if (createProjectDto == null)
        {
            throw new ArgumentNullException(nameof(createProjectDto));
        }

        _logger.LogInformation("Creating new project with name: {ProjectName}", createProjectDto.Name);

        // Check if project with same name already exists
        var existingProject = await _context.Projects
            .FirstOrDefaultAsync(p => p.Name.ToLower() == createProjectDto.Name.ToLower());

        if (existingProject != null)
        {
            _logger.LogWarning("Project creation failed: Project with name {ProjectName} already exists", createProjectDto.Name);
            throw new InvalidOperationException($"A project with the name '{createProjectDto.Name}' already exists.");
        }

        // Create new project entity
        var project = new Project
        {
            Name = createProjectDto.Name.Trim(),
            Description = createProjectDto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        try
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created project with ID: {ProjectId}", project.Id);

            // Map to DTO
            return MapToProjectDto(project);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating project: {ProjectName}", createProjectDto.Name);
            throw new InvalidOperationException("An error occurred while saving the project. Please try again.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        _logger.LogInformation("Retrieving all projects");

        try
        {
            var projects = await _context.Projects
                .Include(p => p.Sessions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {ProjectCount} projects", projects.Count);

            return projects.Select(MapToProjectDtoWithSessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all projects");
            throw new InvalidOperationException("An error occurred while retrieving projects. Please try again.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectDto updateProjectDto)
    {
        if (updateProjectDto == null)
        {
            throw new ArgumentNullException(nameof(updateProjectDto));
        }

        if (id <= 0)
        {
            throw new ArgumentException("Invalid project ID.", nameof(id));
        }

        _logger.LogInformation("Updating project with ID: {ProjectId}", id);

        // Find the existing project
        var project = await _context.Projects
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            _logger.LogWarning("Update failed: Project with ID {ProjectId} not found", id);
            throw new InvalidOperationException($"Project with ID {id} not found.");
        }

        // Check if new name conflicts with another project
        var existingProject = await _context.Projects
            .FirstOrDefaultAsync(p => p.Name.ToLower() == updateProjectDto.Name.ToLower() && p.Id != id);

        if (existingProject != null)
        {
            _logger.LogWarning("Update failed: Project with name {ProjectName} already exists", updateProjectDto.Name);
            throw new InvalidOperationException($"A project with the name '{updateProjectDto.Name}' already exists.");
        }

        try
        {
            // Update project properties
            project.Name = updateProjectDto.Name.Trim();
            project.Description = updateProjectDto.Description?.Trim();
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated project with ID: {ProjectId}", project.Id);

            return MapToProjectDtoWithSessions(project);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating project with ID: {ProjectId}", id);
            throw new InvalidOperationException("An error occurred while updating the project. Please try again.", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteProjectAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Invalid project ID.", nameof(id));
        }

        _logger.LogInformation("Deleting project with ID: {ProjectId}", id);

        // Find the existing project
        var project = await _context.Projects
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            _logger.LogWarning("Delete failed: Project with ID {ProjectId} not found", id);
            throw new InvalidOperationException($"Project with ID {id} not found.");
        }

        try
        {
            // Remove the project (cascade delete will handle sessions)
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted project with ID: {ProjectId} and {SessionCount} associated sessions", 
                id, project.Sessions.Count);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while deleting project with ID: {ProjectId}", id);
            throw new InvalidOperationException("An error occurred while deleting the project. Please try again.", ex);
        }
    }

    /// <summary>
    /// Maps a Project entity to ProjectDto
    /// </summary>
    private static ProjectDto MapToProjectDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TotalSessions = 0, // New project has no sessions
            TotalHours = 0
        };
    }

    /// <summary>
    /// Maps a Project entity with Sessions to ProjectDto including session statistics
    /// </summary>
    private static ProjectDto MapToProjectDtoWithSessions(Project project)
    {
        var totalHours = project.Sessions
            .Where(s => s.EndTime.HasValue)
            .Sum(s => (s.EndTime!.Value - s.StartTime).TotalHours);

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TotalSessions = project.Sessions.Count,
            TotalHours = (int)Math.Round(totalHours)
        };
    }
}
