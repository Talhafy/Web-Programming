using Microsoft.AspNetCore.Mvc;

namespace FitnessCenter.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
