using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaundryApi.Data;
using LaundryApi.Models;

namespace LaundryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/orders (ดึงข้อมูลออเดอร์ทั้งหมด)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            // ใช้ AsNoTracking เพื่อลด overhead สำหรับข้อมูล อ่านอย่างเดียว (Read-only)
            return await _context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // GET: api/orders/5 (ดึงข้อมูลออเดอร์รายรายการ)
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(new { message = $"ไม่พบออเดอร์รหัส {id}" });
            }

            return order;
        }

        // POST: api/orders (สร้างออเดอร์ใหม่)
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // ชี้ URI ไปยัง GetOrder (Single Entity)
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // POST: api/orders/bulk (สร้างออเดอร์ทีละหลายรายการ)
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateOrdersBulk([FromBody] List<Order> orders)
        {
            if (orders == null || !orders.Any())
            {
                return BadRequest(new { message = "กรุณาส่งข้อมูลออเดอร์อย่างน้อย 1 รายการ" });
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrders), new { count = orders.Count }, new { message = $"บันทึกข้อมูลสำเร็จ {orders.Count} รายการ", data = orders });
        }

        // DELETE: api/orders/5 (ลบออเดอร์ตาม ID ที่ระบุ)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var rowsAffected = await _context.Orders
                    .Where(o => o.Id == id)
                    .ExecuteDeleteAsync();

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = $"ไม่พบรายการรหัส {id}" });
                }

                return Ok(new { message = "ลบข้อมูลสำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "เกิดข้อผิดพลาดในการลบข้อมูล", detail = ex.Message });
            }
        }

        // PATCH: api/orders/11/status (อัปเดตเฉพาะสถานะ)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                return BadRequest(new { message = "กรุณาระบุสถานะที่ต้องการอัปเดต" });
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(new { message = $"ไม่พบออเดอร์รหัส {id}" });
            }

            order.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "อัปเดตสถานะสำเร็จ", data = order });
        }
    }

    // DTO สำหรับรับค่า Status จาก Request Body
    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}