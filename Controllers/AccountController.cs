using Microsoft.AspNetCore.Mvc;
//Controller mẫu để chạy giao diện thoi 
namespace LoginDemo.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
    }
}
