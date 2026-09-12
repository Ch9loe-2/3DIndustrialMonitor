using System.ComponentModel.DataAnnotations;

namespace IndustrialMonitorAPI.Models;

public class AlarmRecord
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string DeviceName { get; set; } = "";

    [MaxLength(50)]
    public string AlarmType { get; set; } = "";

    [MaxLength(20)]
    public string AlarmLevel { get; set; } = "";

    [MaxLength(500)]
    public string AlarmMessage { get; set; } = "";

    public DateTime Time { get; set; } = DateTime.UtcNow;

    /// <summary>未恢复 / 已恢复</summary>
    [MaxLength(20)]
    public string Status { get; set; } = "未恢复";

    public DateTime? RecoverTime { get; set; }
}