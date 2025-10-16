using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevFlow.Utilities;

namespace DevFlow.Models;

/// <summary>
/// Represents a coding session tracked for a specific project
/// </summary>
public class Session
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public DateTime StartTime { get; set; } = DateTimeHelper.Now;

    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Duration in seconds. Calculated when session ends.
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Indicates if the session is currently active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if the session was automatically ended due to idle detection
    /// </summary>
    public bool IsAutoStopped { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now;

    // Navigation property
    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;
}
