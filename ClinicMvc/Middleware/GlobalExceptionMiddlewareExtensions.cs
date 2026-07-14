namespace ClinicMvc.Middleware;

/// <summary>
/// Extension метод кој овозможува чисто и читливо регистрирање на
/// GlobalExceptionMiddleware во Program.cs (наместо app.UseMiddleware&lt;...&gt;() директно).
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// Го регистрира GlobalExceptionMiddleware во HTTP pipeline-от.
    /// Употреба во Program.cs: app.UseGlobalExceptionHandling();
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
