using Microsoft.EntityFrameworkCore;
using IndustrialMonitorAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// ----- 服务注册 -----

// 控制器
builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=industrial_monitor.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

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

// ----- 中间件管道 -----

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowUnity");

app.MapControllers();

// ----- 自动建库 + 种子数据 -----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();