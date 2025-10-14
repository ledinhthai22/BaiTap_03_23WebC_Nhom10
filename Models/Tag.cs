using System.ComponentModel.DataAnnotations;

namespace BaiTap_03_23WebC_Nhom10.Models
{
    public class Tag
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Tên tag là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên tag tối đa 255 ký tự")]
        public string tagName { get; set; } = string.Empty;
    }
}
