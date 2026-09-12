using Microsoft.EntityFrameworkCore;
using IndustrialMonitorAPI.Models;

namespace IndustrialMonitorAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<AlarmRecord> AlarmRecords => Set<AlarmRecord>();
    public DbSet<DeviceMetricHistory> DeviceMetricHistories => Set<DeviceMetricHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 历史数据查询索引：按设备 + 指标 + 时间范围检索
        modelBuilder.Entity<DeviceMetricHistory>()
            .HasIndex(h => new { h.DeviceName, h.MetricName, h.Timestamp });

        // 设备表初始数据（3 台设备）
        // 注意：HasData 必须使用静态值，避免使用 DateTime.UtcNow 等动态默认值，
        // 否则每次模型构建生成的种子值不同，会触发 PendingModelChangesWarning。
        var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Device>().HasData(
            new Device
            {
                Id = 1,
                DeviceName = "设备 A",
                DeviceType = "生产设备",
                Workshop = "一号车间",
                Temperature = 65.2f,
                Pressure = 1.62f,
                Rpm = 1450,
                Runtime = 128.5f,
                Status = "正常",
                InitialTemperature = 65.2f,
                InitialPressure = 1.62f,
                InitialRpm = 1450,
                InitialRuntime = 128.5f,
                CreatedAt = seedTime,
                UpdatedAt = seedTime,
            },
            new Device
            {
                Id = 2,
                DeviceName = "设备 B",
                DeviceType = "生产设备",
                Workshop = "一号车间",
                Temperature = 68.5f,
                Pressure = 1.70f,
                Rpm = 1500,
                Runtime = 96.3f,
                Status = "正常",
                InitialTemperature = 68.5f,
                InitialPressure = 1.70f,
                InitialRpm = 1500,
                InitialRuntime = 96.3f,
                CreatedAt = seedTime,
                UpdatedAt = seedTime,
            },
            new Device
            {
                Id = 3,
                DeviceName = "设备 C",
                DeviceType = "生产设备",
                Workshop = "二号车间",
                Temperature = 61.8f,
                Pressure = 1.55f,
                Rpm = 1380,
                Runtime = 210.7f,
                Status = "正常",
                InitialTemperature = 61.8f,
                InitialPressure = 1.55f,
                InitialRpm = 1380,
                InitialRuntime = 210.7f,
                CreatedAt = seedTime,
                UpdatedAt = seedTime,
            }
        );
    }
}