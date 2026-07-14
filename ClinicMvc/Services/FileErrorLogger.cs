using System.Text;

namespace ClinicMvc.Services;

/// <summary>
/// Имплементација на IErrorLogger која ги запишува грешките во текстуална датотека
/// на патеката Logs/errors.txt во коренот на проектот.
/// Регистриран е како Singleton во DI бидејќи чува само патека до датотеката (нема состојба по барање).
/// </summary>
public class FileErrorLogger : IErrorLogger
{
    private readonly string _logFilePath;

    // SemaphoreSlim спречува повеќе паралелни барања да пишуваат во датотеката
    // во ист момент (што би довело до испреплетени/оштетени записи).
    private static readonly SemaphoreSlim FileLock = new(1, 1);

    /// <summary>
    /// Конструктор - IWebHostEnvironment се инјектира автоматски преку DI
    /// и се користи за да се најде коренот на проектот (ContentRootPath).
    /// </summary>
    public FileErrorLogger(IWebHostEnvironment environment)
    {
        var logsFolder = Path.Combine(environment.ContentRootPath, "Logs");

        // Ако папката Logs не постои, креирај ја автоматски при стартување
        Directory.CreateDirectory(logsFolder);

        _logFilePath = Path.Combine(logsFolder, "errors.txt");
    }

    /// <summary>
    /// Го запишува исклучокот во Logs/errors.txt со сите потребни детали:
    /// датум/време, порака, stack trace, URL, контролер и акција.
    /// </summary>
    public async Task LogErrorAsync(Exception exception, HttpContext context)
    {
        // Ги земаме контролерот и акцијата од route вредностите (ако се веќе резолвирани)
        var routeValues = context.Request.RouteValues;
        var controller  = routeValues.TryGetValue("controller", out var c) ? c?.ToString() : "N/A";
        var action      = routeValues.TryGetValue("action", out var a) ? a?.ToString() : "N/A";

        // Составуваме еден читлив запис за логот
        var entry = new StringBuilder();
        entry.AppendLine("========================================================");
        entry.AppendLine($"Дата и време: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        entry.AppendLine($"HTTP метод:   {context.Request.Method}");
        entry.AppendLine($"URL:          {context.Request.Path}{context.Request.QueryString}");
        entry.AppendLine($"Контролер:    {controller}");
        entry.AppendLine($"Акција:       {action}");
        entry.AppendLine($"Тип грешка:   {exception.GetType().FullName}");
        entry.AppendLine($"Порака:       {exception.Message}");
        entry.AppendLine("Stack Trace:");
        entry.AppendLine(exception.StackTrace ?? "(нема достапен stack trace)");

        // Ако постои внатрешен исклучок (InnerException), логирај го и него
        if (exception.InnerException != null)
        {
            entry.AppendLine("Внатрешна грешка (InnerException):");
            entry.AppendLine(exception.InnerException.Message);
        }

        entry.AppendLine("========================================================");
        entry.AppendLine();

        // Чекаме ред за пишување за да избегнеме конфликт меѓу паралелни барања
        await FileLock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(_logFilePath, entry.ToString());
        }
        finally
        {
            FileLock.Release();
        }
    }
}
