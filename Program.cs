using Microsoft.EntityFrameworkCore;
using LaundryApi.Data;

var builder = WebApplication.CreateBuilder(args);

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

// 2. เปิดใช้งาน Swagger ทุก Environment (เพื่อให้เปิดทดสอบบน Render ได้)
app.UseSwagger();
app.UseSwaggerUI();

// 3. เรียกใช้งาน CORS Policy ที่ตั้งชื่อไว้ว่า "AllowAll"
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();