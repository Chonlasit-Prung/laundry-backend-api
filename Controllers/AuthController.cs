using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaundryApi.Data;
using BCrypt.Net;

namespace LaundryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        // ฉีด ILogger เข้ามาเพื่อใช้ในการบันทึก Log
        public AuthController(AppDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/auth/verify-password
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { success = false, message = "กรุณากรอกรหัสผ่าน" });
            }

            try
            {
                var setting = await _context.Settings.FirstOrDefaultAsync();

                if (setting == null || string.IsNullOrEmpty(setting.ShopPassword))
                {
                    return NotFound(new { success = false, message = "ไม่พบข้อมูลการตั้งค่าในระบบ" });
                }

                bool isValid = false;

                try
                {
                    // ตรวจสอบรหัสผ่านผ่าน BCrypt
                    isValid = BCrypt.Net.BCrypt.Verify(dto.Password, setting.ShopPassword);
                }
                catch (Exception ex)
                {
                    // บันทึก Log เมื่อรหัสผ่านผิด หรือรูปแบบ Hash ใน DB ไม่ถูกต้อง
                    _logger.LogError(ex, "BCrypt verification failed or hash format is invalid.");
                    isValid = false;
                }

                if (!isValid)
                {
                    return Unauthorized(new { success = false, message = "รหัสผ่านไม่ถูกต้อง" });
                }

                return Ok(new { success = true, message = "รหัสผ่านถูกต้อง" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database or server error occurred.");
                return StatusCode(500, new { success = false, message = $"Server Error: {ex.Message}" });
            }
        }
    }

    public class VerifyPasswordDto
    {
        public string Password { get; set; } = string.Empty;
    }
}