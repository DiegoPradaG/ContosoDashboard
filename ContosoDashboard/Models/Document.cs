using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class Document
{
    [Key]
    public int DocumentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int UploadedByUserId { get; set; }

    public int? ProjectId { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    // Navigation properties
    [ForeignKey("UploadedByUserId")]
    public virtual User UploadedByUser { get; set; } = null!;

    [ForeignKey("ProjectId")]
    public virtual Project? Project { get; set; }

    public int? TaskId { get; set; }

    [ForeignKey("TaskId")]
    public virtual TaskItem? Task { get; set; }

    public virtual ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
}
