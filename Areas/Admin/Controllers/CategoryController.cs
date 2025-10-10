using Microsoft.AspNetCore.Mvc;
using BaiTap_03_23WebC_Nhom10.Filters;//Bổ sung thêm namspace để dùng filter áa
namespace BaiTap_03_23WebC_Nhom10.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeRole("Admin")]//Bổ sung hạn chế chỉ có admin mới có thể gọi controller nì 
    [Route("Admin/[Controller]")]
    public class CategoryController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Them-danh-muc")]
        public IActionResult Create()
        {
            return View();
        }
    }
}
