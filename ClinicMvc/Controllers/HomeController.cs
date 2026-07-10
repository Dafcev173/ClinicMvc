using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
