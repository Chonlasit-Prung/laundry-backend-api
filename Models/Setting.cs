//Setting.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LaundryApi.Models
{
    [Table("settings")]
    public class Setting
    {
        [Key]
        [Column("unique_id")]
        public int UniqueId {get; set;}

        [Column("shop_password")]
        public string ShopPassword { get; set; } = string.Empty;
    }
}