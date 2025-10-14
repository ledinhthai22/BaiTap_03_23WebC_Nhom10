using BaiTap_03_23WebC_Nhom10.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BaiTap_03_23WebC_Nhom10.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeRole("Admin")]
    [Route("Admin/[controller]")]
    public class TagController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("them-tag")]
        public IActionResult Create()
        {
            return View();
        }
    }
}
