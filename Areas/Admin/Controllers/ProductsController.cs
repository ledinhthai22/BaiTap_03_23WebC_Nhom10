using BaiTap_03_23WebC_Nhom10.Models;
using Microsoft.AspNetCore.Mvc;
using BaiTap_03_23WebC_Nhom10.Service;
using BaiTap_03_23WebC_Nhom10.Filters;//Bổ sung thêm namspace để dùng filter áa
namespace BaiTap_03_23WebC_Nhom10.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeRole("Admin")]//Bổ sung hạn chế chỉ có admin mới có thể gọi controller nì 
    [Route("Admin/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ProductService _productService;

        public ProductsController(IWebHostEnvironment env, ProductService productService)
        {
            _env = env;
            _productService = productService;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            var list = _productService.GetAll();
            return View(list);
        }
        [HttpGet("tao-san-pham")]

        public IActionResult Create()
        {
            return View();
        }

    }
}