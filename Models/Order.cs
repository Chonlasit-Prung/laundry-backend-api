using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryApi.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_name")]
        public string? CustomerName { get; set; }

        [Column("shirt_qty")]
        public int ShirtQty { get; set; }

        [Column("pant_qty")]
        public int PantQty { get; set; }

        [Column("details")]
        public string? Details { get; set; }

        //  เพิ่ม TypeName = "text" รองรับ Base64 ยาวๆ
        [Column("cloth_image_url", TypeName = "text")]
        public string? ClothImageUrl { get; set; }

        [Column("pickup_date")]
        public DateTime? PickupDate { get; set; }

        //  เพิ่ม TypeName = "text" รองรับ Base64 ยาวๆ
        [Column("payment_slip_url", TypeName = "text")]
        public string? PaymentSlipUrl { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("phone")]
        public string? Phone { get; set; }
    }
}