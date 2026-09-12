using System.ComponentModel.DataAnnotations;

namespace IndustrialMonitorAPI.Models;

public class Device
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string DeviceName { get; set; } = "设备 A";

    [MaxLength(50)]
    public string DeviceType { get; set; } = "生产设备";

    public float Temperature { get; set; } = 65.2f;

    public float Pressure { get; set; } = 1.62f;

    public int Rpm { get; set; } = 1450;

    public float Runtime { get; set; } = 128.5f;

    [MaxLength(20)]
    public string Status { get; set; } = "正常";

    /// <summary>初始值（恢复时回到此值）</summary>
    public float InitialTemperature { get; set; } = 65.2f;

    public float InitialPressure { get; set; } = 1.62f;

    public int InitialRpm { get; set; } = 1450;

    public float InitialRuntime { get; set; } = 128.5f;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}