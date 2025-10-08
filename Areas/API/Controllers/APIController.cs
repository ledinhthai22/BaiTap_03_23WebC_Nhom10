using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.AspNetCore.Mvc;

namespace BaiTap_03_23WebC_Nhom10.Areas.API.Controllers
{
    public class APIController : Controller
    {
        private readonly ProductService _productService;
        public APIController(ProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public IActionResult GetProducts()
        {
            List<Product> products = new List<Product>();

            if (HttpContext.Items.TryGetValue("products", out var productsObj) && productsObj is List<Product> p)
            {
                products = p;
            }
            return Json(products);
        }
    }
}
