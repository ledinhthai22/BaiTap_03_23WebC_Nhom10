using Microsoft.AspNetCore.Mvc;

namespace BaiTap_03_23WebC_Nhom10.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        [Area("Admin")]
        [Route("Admin/[Controller]")]
        public IActionResult Index()
        {
            return View();
        }
     
    }
}
