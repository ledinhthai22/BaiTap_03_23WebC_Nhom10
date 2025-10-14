using System.ComponentModel.DataAnnotations;

namespace BaiTap_03_23WebC_Nhom10.Models
{
    public class Category
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên danh mục tối đa 255 ký tự")]
        public string categoryName { get; set; } = string.Empty;

        [Required]
        public bool status { get; set; }
    }
}
