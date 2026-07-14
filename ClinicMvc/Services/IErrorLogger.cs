namespace ClinicMvc.Services;

/// <summary>
/// Интерфејс за логирање на грешки.
/// Овозможува лесна замена на имплементацијата (пр. текстуална датотека,
/// база на податоци, надворешен сервис) без да се менува повикувачкиот код.
/// </summary>
public interface IErrorLogger
{
    /// <summary>
    /// Логира исклучок заедно со контекстот на HTTP барањето во кое се случил.
    /// </summary>
    Task LogErrorAsync(Exception exception, HttpContext context);
}
