using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;



/// <summary>
/// Контролер за почетната страница и страницата за грешки.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// GET: /
    /// Ја прикажува почетната страница со линкови кон Доктори/Пациенти/Термини.
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// GET: /Home/Error
    /// Пријателска страница за грешки - кон неа пренасочува GlobalExceptionMiddleware
    /// секогаш кога ќе се фати необработен исклучок во апликацијата (за не-AJAX барања).
    /// [ResponseCache] со NoStore спречува прелистувачот да ја кешира оваа страница -
    /// секоја грешка треба да се прикаже свежо, не од кеш.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
