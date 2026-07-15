using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

/// <summary>
/// Контролер за почетната страница и страницата за грешки.
/// [AllowAnonymous] е потребен бидејќи глобалната политика (Program.cs) бара
/// најава за сите контролери по default - овие две акции мора да бидат исклучок.
/// </summary>
public class HomeController : Controller
{
    /// <summary>GET: / - ако не е најавен, автоматски ќе биде пренасочен кон Login.</summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// GET: /Home/Error
    /// Мора да е достапна и за не-најавени корисници (грешка може да се случи и пред најава).
    /// </summary>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
