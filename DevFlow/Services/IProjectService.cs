using DevFlow.Models.DTOs;

namespace DevFlow.Services;

/// <summary>
/// Interface defining project-related operations
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Creates a new project with the provided details
    /// </summary>
    /// <param name="createProjectDto">Project creation details</param>
    /// <returns>The created project details</returns>
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto);

    /// <summary>
    /// Retrieves all projects with their summary information
    /// </summary>
    /// <returns>A list of all projects with session statistics</returns>
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
}
