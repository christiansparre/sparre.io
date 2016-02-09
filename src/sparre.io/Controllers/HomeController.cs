using Microsoft.AspNet.Mvc;

namespace sparre.io.Controllers
{
    public class HomeController : Controller
    {
        
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}