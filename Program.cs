//Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using LaundryApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ปิด reloadOnChange เพื่อป้องกัน Crash (Exit code 139) บน Linux/Render
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// ตั้งค่า CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://laundry-frontend-ivory.vercel.app/", // เปลี่ยนเป็น Domain จริงของ Frontend                // (ถ้ามี) Custom Domain
                "http://localhost:4200"                     // เพิ่มไว้สำหรับเทสบนเครื่อง Local (เช่น Vite/React/Vue)
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // ใส่เพิ่มกรณีมีการส่ง Cookie หรือ Credentials
    });
});

// เชื่อมต่อ Database พร้อมระบบ Retry & Timeout
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(60); 
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    });

    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// เปิดใช้งาน Swagger ทุก Environment
app.UseSwagger();
app.UseSwaggerUI();

//CORS
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();