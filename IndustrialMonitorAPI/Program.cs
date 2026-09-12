using IndustrialMonitorAPI.Common;
using IndustrialMonitorAPI.Data;
using IndustrialMonitorAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 服务注册
// ============================================================

// 控制器 + 统一参数校验失败响应格式
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = new ApiResponse<object>(
                400,
                errors.Count > 0 ? string.Join("；", errors) : "请求参数校验失败",
                null);

            return new BadRequestObjectResult(response);
        };
    });

// OpenAPI / Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=industrial_monitor.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// 业务服务
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<AlarmService>();
builder.Services.AddScoped<MetricHistoryService>();

// CORS：允许 Unity Editor (localhost) 调用
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnity", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ============================================================
// 中间件管道
// ============================================================

// 全局异常处理：统一返回 ApiResponse 格式，避免暴露内部异常
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>(500, "服务器内部发生错误", null);
        await context.Response.WriteAsJsonAsync(response);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowUnity");

app.MapControllers();

// 默认首页
app.MapGet("/", () => "工业设备监控后端已启动！请访问 /swagger 查看接口文档。");

// ============================================================
// 自动建库 + 种子数据
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

// 供集成测试使用（WebApplicationFactory 需要入口点可见）
public partial class Program { }
