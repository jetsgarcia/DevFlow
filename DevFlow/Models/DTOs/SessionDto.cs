using System.ComponentModel.DataAnnotations;

namespace DevFlow.Models.DTOs;

/// <summary>
/// DTO for starting a new session
/// </summary>
public class StartSessionDto
{
    [Required(ErrorMessage = "ProjectId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be a positive integer")]
    public int ProjectId { get; set; }
}

/// <summary>
/// DTO for session response
/// </summary>
public class SessionDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsActive { get; set; }
    public bool IsAutoStopped { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for ending a session
/// </summary>
public class EndSessionDto
{
    [Required(ErrorMessage = "SessionId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "SessionId must be a positive integer")]
    public int SessionId { get; set; }
}

/// <summary>
/// DTO for active session response
/// </summary>
public class ActiveSessionDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int ElapsedSeconds { get; set; }
}
