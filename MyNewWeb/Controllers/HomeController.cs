using Microsoft.AspNetCore.Mvc;

namespace MyNewWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
