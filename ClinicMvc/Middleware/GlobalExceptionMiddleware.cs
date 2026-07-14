using System.Text.Json;
using ClinicMvc.Services;

namespace ClinicMvc.Middleware;

/// <summary>
/// Middleware кој фаќа СИТЕ необработени исклучоци од целата апликација
/// (сите контролери и акции), без потреба секој контролер да се менува посебно.
///
/// Работи вака:
/// 1. Го "обвиткува" остатокот на pipeline-от во try/catch.
/// 2. Ако се фати исклучок - го логира во Logs/errors.txt преку IErrorLogger.
/// 3. За AJAX барања - враќа чист JSON одговор (компатибилен со постоечкиот
///    .fail(xhr => xhr.responseJSON?.errors) шаблон што веќе го користи целата апликација).
/// 4. За обични страници - пренасочува кон пријателска Error страница.
///
/// Никогаш не ги прикажува техничките детали (порака/stack trace) на корисникот.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _consoleLogger;

    /// <summary>
    /// RequestDelegate е следниот чекор во pipeline-от (следното middleware/контролер).
    /// ILogger е вградениот ASP.NET Core логер - го користиме дополнително за конзолата,
    /// покрај нашиот сопствен IErrorLogger кој пишува во датотека.
    /// </summary>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> consoleLogger)
    {
        _next = next;
        _consoleLogger = consoleLogger;
    }

    /// <summary>
    /// Главниот метод кој ASP.NET Core го повикува за секое HTTP барање.
    /// IErrorLogger се инјектира тука (per-request), а не во конструкторот,
    /// бидејќи middleware конструкторот се повикува само еднаш (Singleton lifetime),
    /// додека IErrorLogger треба да работи безбедно по барање.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IErrorLogger errorLogger)
    {
        try
        {
            // Продолжи со остатокот на pipeline-от (routing, контролери, акции...)
            await _next(context);
        }
        catch (Exception ex)
        {
            // Логирај ја грешката во конзолата (за развој) и во текстуалната датотека
            _consoleLogger.LogError(ex, "Необработена грешка на {Path}", context.Request.Path);
            await errorLogger.LogErrorAsync(ex, context);

            // Ако одговорот веќе почнал да се испраќа кон клиентот, не можеме да го смениме -
            // единствената опција е да продолжиме со фрлање (серверот ќе го затвори барањето)
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();

            if (IsAjaxRequest(context.Request))
            {
                // AJAX барање - врати чист JSON со пријателска порака (без технички детали)
                context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json; charset=utf-8";

                var payload = JsonSerializer.Serialize(new
                {
                    success = false,
                    errors = new[] { "Настана неочекувана грешка. Обидете се повторно подоцна." }
                });

                await context.Response.WriteAsync(payload);
            }
            else
            {
                // Обично барање (полно вчитување на страница) - пренасочи кон Error страницата
                context.Response.Redirect("/Home/Error");
            }
        }
    }

    /// <summary>
    /// Проверува дали барањето е AJAX повик.
    /// jQuery автоматски го поставува "X-Requested-With: XMLHttpRequest" header-от
    /// за секој $.get/$.post повик, што веќе ги користиме низ целата апликација.
    /// </summary>
    private static bool IsAjaxRequest(HttpRequest request)
    {
        return request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || request.Headers.Accept.ToString().Contains("application/json");
    }
}
