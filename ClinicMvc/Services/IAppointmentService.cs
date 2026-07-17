using ClinicMvc.Models;

namespace ClinicMvc.Services;

/// <summary>
/// Дефинира бизнис логиката за термини - валидации, конфликти, статус преоди.
/// Контролерот само ги повикува овие методи и одлучува како да го обликува одговорот.
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// Ги проверува деловните правила (активен доктор, минат датум, конфликт термин)
    /// и креира термин ако сите проверки поминат. Враќа листа на грешки (празна = успех).
    /// </summary>
    Task<(bool Success, int NewId, IEnumerable<string> Errors)> CreateAppointmentAsync(Appointment appointment, string username);

    Task<(bool Success, IEnumerable<string> Errors)> UpdateAppointmentAsync(Appointment appointment, string username);

    Task DeleteAppointmentAsync(int id, string username);

    Task<(bool Success, IEnumerable<string> Errors)> StartExamAsync(int id, string username);

    Task<(bool Success, IEnumerable<string> Errors)> FinishExamAsync(int id, string? notes, string username);
}
