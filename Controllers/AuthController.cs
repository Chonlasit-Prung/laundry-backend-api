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

            try
            {
                var setting = await _context.Settings.FirstOrDefaultAsync();

                if (setting == null || string.IsNullOrEmpty(setting.ShopPassword))
                {
                    return NotFound(new { success = false, message = "ไม่พบข้อมูลการตั้งค่าในระบบ" });
                }

                bool isValid = false;

                // ตรวจสอบว่าเป็น BCrypt Hash หรือไม่ (ขึ้นต้นด้วย $2)
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
                    // กรณีใน DB เก็บเป็น Plain Text ตรงๆ (เช่น "249918")
                    isValid = (dto.Password == setting.ShopPassword);
                }

                if (!isValid)
                {
                    return Unauthorized(new { success = false, message = "รหัสผ่านไม่ถูกต้อง" });
                }

                return Ok(new { success = true, message = "รหัสผ่านถูกต้อง" });
            }
            catch (Exception ex)
            {
                // ดักจับกรณี DB Error หรือ Table หาไม่เจอ
                return StatusCode(500, new { success = false, message = $"Server Error: {ex.Message}" });
            }
        }
    }

    public class VerifyPasswordDto
    {
        public string Password { get; set; } = string.Empty;
    }
}