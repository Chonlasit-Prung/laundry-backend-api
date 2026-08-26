using Microsoft.EntityFrameworkCore;
using LaundryApi.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔹 ปิด reloadOnChange เพื่อป้องกัน Crash (Exit code 139) บน Linux/Render
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// 1. ตั้งค่า CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔹 ทำการ Auto-Migrate Database เมื่อ App เริ่มทำงานบน Render
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 2. เปิดใช้งาน Swagger ทุก Environment
app.UseSwagger();
app.UseSwaggerUI();

// 3. เรียกใช้งาน CORS
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();