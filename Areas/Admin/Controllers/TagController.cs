using Microsoft.AspNetCore.Mvc;

namespace BaiTap_03_23WebC_Nhom10.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class TagController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
