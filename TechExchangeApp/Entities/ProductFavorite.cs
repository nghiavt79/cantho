using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechExchangeApp.Entities
{
    /// <summary>Sản phẩm người dùng đánh dấu "Quan tâm" (HSMT B.3.3).</summary>
    [Table("ProductFavorites")]
    public class ProductFavorite
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int SanPhamId { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;
    }
}
