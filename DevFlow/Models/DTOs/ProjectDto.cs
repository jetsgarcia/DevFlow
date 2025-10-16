using System.ComponentModel.DataAnnotations;

namespace DevFlow.Models.DTOs;

/// <summary>
/// DTO for creating a new project
/// </summary>
public class CreateProjectDto
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Project name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}

/// <summary>
/// DTO for updating an existing project
/// </summary>
public class UpdateProjectDto
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Project name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}

/// <summary>
/// DTO for project response
/// </summary>
public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TotalSessions { get; set; }
    public int TotalHours { get; set; }
}

/// <summary>
/// DTO for project summary (list view)
/// </summary>
public class ProjectSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalSessions { get; set; }
    public double TotalHours { get; set; }
}
