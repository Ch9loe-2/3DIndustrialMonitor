using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IndustrialMonitorAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlarmRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AlarmType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AlarmLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AlarmMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RecoverTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceMetricHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MetricName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMetricHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DeviceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Temperature = table.Column<float>(type: "REAL", nullable: false),
                    Pressure = table.Column<float>(type: "REAL", nullable: false),
                    Rpm = table.Column<int>(type: "INTEGER", nullable: false),
                    Runtime = table.Column<float>(type: "REAL", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    InitialTemperature = table.Column<float>(type: "REAL", nullable: false),
                    InitialPressure = table.Column<float>(type: "REAL", nullable: false),
                    InitialRpm = table.Column<int>(type: "INTEGER", nullable: false),
                    InitialRuntime = table.Column<float>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Devices",
                columns: new[] { "Id", "CreatedAt", "DeviceName", "DeviceType", "InitialPressure", "InitialRpm", "InitialRuntime", "InitialTemperature", "Pressure", "Rpm", "Runtime", "Status", "Temperature", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "设备 A", "生产设备", 1.62f, 1450, 128.5f, 65.2f, 1.62f, 1450, 128.5f, "正常", 65.2f, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "设备 B", "生产设备", 1.7f, 1500, 96.3f, 68.5f, 1.7f, 1500, 96.3f, "正常", 68.5f, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "设备 C", "生产设备", 1.55f, 1380, 210.7f, 61.8f, 1.55f, 1380, 210.7f, "正常", 61.8f, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMetricHistories_DeviceName_MetricName_Timestamp",
                table: "DeviceMetricHistories",
                columns: new[] { "DeviceName", "MetricName", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlarmRecords");

            migrationBuilder.DropTable(
                name: "DeviceMetricHistories");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
