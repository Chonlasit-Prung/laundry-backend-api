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
            // 1. ตรวจสอบว่ามีการส่งรหัสผ่านเข้ามาหรือไม่
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { success = false, message = "กรุณากรอกรหัสผ่าน" });
            }

            // 2. ดึงข้อมูล Settings จาก Database
            var setting = await _context.Settings.FirstOrDefaultAsync();

            if (setting == null)
            {
                return NotFound(new { success = false, message = "ไม่พบข้อมูลการตั้งค่าในระบบ" });
            }

            // 3. นำรหัสผ่าน Plaintext ที่ส่งมาจาก Frontend/Swagger ไป Verify กับ BCrypt Hash ใน DB
            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, setting.ShopPassword);

            // 4. ถ้ารหัสผ่านไม่ตรง Return 401
            if (!isValid)
            {
                return Unauthorized(new { success = false, message = "รหัสผ่านไม่ถูกต้อง" });
            }

            // 5. ถ้ารหัสผ่านถูกต้อง Return 200 OK
            return Ok(new { success = true, message = "รหัสผ่านถูกต้อง" });
        }
    }

    public class VerifyPasswordDto
    {
        public string Password { get; set; } = string.Empty;
    }
}