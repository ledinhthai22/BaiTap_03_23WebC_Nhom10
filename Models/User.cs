using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace BaiTap_03_23WebC_Nhom10.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(255)]
        public string userName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255)]
        public string passWord { get; set; } = string.Empty;

        [ForeignKey("Role")]
        public int roleID { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255)]
        public string? email { get; set; }

        public bool status { get; set; } = true;

        public DateTime? createAt { get; set; } = DateTime.Now;

        public DateTime? updateAt { get; set; }
    }
}
