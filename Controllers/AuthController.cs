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

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/verify-password
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { success = false, message = "กรุณากรอกรหัสผ่าน" });
            }

            // ดึงข้อมูลรหัสผ่านจากแถวแรกในตาราง settings
            var setting = await _context.Settings.FirstOrDefaultAsync();

            if (setting == null)
            {
                return NotFound(new { success = false, message = "ไม่พบข้อมูลการตั้งค่าในระบบ" });
            }

            // ตรวจสอบรหัสผ่าน plaintext กับ BCrypt hash ใน DB
            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, setting.ShopPassword);

            if (!isValid)
            {
                return Unauthorized(new { success = false, message = "รหัสผ่านไม่ถูกต้อง" });
            }

            return Ok(new { success = true, message = "รหัสผ่านถูกต้อง" });
        }
    }

    public class VerifyPasswordDto
    {
        public string Password { get; set; } = string.Empty;
    }
}