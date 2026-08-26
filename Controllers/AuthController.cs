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

            var setting = await _context.Settings.FirstOrDefaultAsync();

            if (setting == null)
            {
                return NotFound(new { success = false, message = "ไม่พบข้อมูลการตั้งค่าในระบบ" });
            }

            bool isValid = false;

            // เช็คว่าค่าใน DB เป็น BCrypt Format (ขึ้นต้นด้วย $2a$, $2b$, $2y$) หรือไม่
            if (setting.ShopPassword.StartsWith("$2"))
            {
                try
                {
                    isValid = BCrypt.Net.BCrypt.Verify(dto.Password, setting.ShopPassword);
                }
                catch
                {
                    isValid = false;
                }
            }
            else
            {
                // หากใน DB เป็นข้อความธรรมดา ให้เทียบตรงๆ แล้วแปลงเป็น Hash บันทึกเก็บทันที
                if (setting.ShopPassword == dto.Password)
                {
                    setting.ShopPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    await _context.SaveChangesAsync();
                    isValid = true;
                }
            }

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