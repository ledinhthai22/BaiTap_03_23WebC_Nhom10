using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaiTap_03_23WebC_Nhom10.Models
{
    public class Product
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(255)]
        public string productName { get; set; } = string.Empty;

        [Range(0, 9999999999999999.99, ErrorMessage = "Giá không hợp lệ")]
        public decimal price { get; set; }

        [Range(0, 100, ErrorMessage = "Giảm giá phải từ 0% đến 100%")]
        public decimal discount { get; set; }

        [StringLength(255)]
        public string? image { get; set; }

        [StringLength(255)]
        public string? description { get; set; }

        [Range(0, int.MaxValue)]
        public int? quality { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int categoryID { get; set; }

        public int? tagID { get; set; }

        public int views { get; set; } = 0;
        public int selled { get; set; } = 0;
        public bool status { get; set; } = true;

        public DateTime createAT { get; set; } = DateTime.Now;
        public DateTime? updateAT { get; set; }

        [NotMapped]
        public string? categoryName { get; set; }

        [NotMapped]
        public List<string>? imageList { get; set; }
    }
}
