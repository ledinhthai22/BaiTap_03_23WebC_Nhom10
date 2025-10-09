using BaiTap_03_23WebC_Nhom10.Models;
using Microsoft.AspNetCore.Mvc;
using BaiTap_03_23WebC_Nhom10.Service;
using PagedList;

namespace WebApplication1.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index(int page = 1, int pageSize = 8)
        {
           
            return View(); 
        }

        public IActionResult Detail(int id)
        {
            /*if (HttpContext.Items.TryGetValue("products", out var productsObj) && productsObj is IEnumerable<Product> products)
            {
                var product = products.FirstOrDefault(p => p.id == id);
                if (product == null) return View("~/Views/Home/NotFound.cshtml");
                return View(product);
            }
            return View("~/Views/Home/NotFound.cshtml");*/
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }
    }
}
