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
}
