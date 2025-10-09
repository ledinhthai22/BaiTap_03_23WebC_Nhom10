using Microsoft.AspNetCore.Mvc;

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
