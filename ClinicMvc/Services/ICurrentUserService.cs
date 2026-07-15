namespace ClinicMvc.Services;

/// <summary>
/// Помошен сервис за читање на податоците за најавениот корисник од HttpContext.
/// Ги избегнува повторените ClaimsPrincipal повици во секој контролер.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Корисничко ime на најавениот корисник (се користи за CreatedBy/ModifiedBy)</summary>
    string Username { get; }

    /// <summary>"Administrator" или "Doctor"</summary>
    string? Role { get; }

    /// <summary>ID на докторот поврзан со сметката (null за администратори)</summary>
    int? DoctorId { get; }

    bool IsAdministrator { get; }
    bool IsDoctor { get; }
}
