using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaundryApi.Data;

namespace LaundryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Auth/verify-password
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { success = false, message = "กรุณากรอกรหัสผ่าน" });
            }

            try
            {
                // 🔹 ใช้ AsNoTracking() เพื่อความเร็ว ไม่ต้องให้ EF Core ทำ Tracking
                var setting = await _context.Settings
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (setting == null || string.IsNullOrEmpty(setting.ShopPassword))
                {
                    return NotFound(new { success = false, message = "ไม่พบข้อมูลการตั้งค่าในระบบ" });
                }

                // เปรียบเทียบรหัสผ่าน
                if (setting.ShopPassword.Trim() != dto.Password.Trim())
                {
                    return Unauthorized(new { success = false, message = "รหัสผ่านไม่ถูกต้อง" });
                }

                return Ok(new { success = true, message = "รหัสผ่านถูกต้อง" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการตรวจสอบรหัสผ่าน");
                
                // 🔹 ซ่อนรายละเอียด Internal Error แต่เก็บ log ไว้
                return StatusCode(500, new { 
                    success = false, 
                    message = "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์ กรุณาลองใหม่อีกครั้ง" 
                });
            }
        }
    }

    public class VerifyPasswordDto
    {
        public string Password { get; set; } = string.Empty;
    }
}