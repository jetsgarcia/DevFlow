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

    /// <summary>
    /// Updates an existing project's details
    /// </summary>
    /// <param name="id">The ID of the project to update</param>
    /// <param name="updateProjectDto">The updated project details</param>
    /// <returns>The updated project details</returns>
    Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectDto updateProjectDto);

    /// <summary>
    /// Deletes an existing project and all associated sessions
    /// </summary>
    /// <param name="id">The ID of the project to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteProjectAsync(int id);
}
