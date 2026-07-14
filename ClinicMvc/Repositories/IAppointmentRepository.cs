using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

/// <summary>
/// Интерфејс за репозиториумот на термини.
/// Дефинира кои операции се достапни за работа со табелата APPOINTMENTS.
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>Ги враќа сите термини без филтри и без пагинација (за интерна употреба)</summary>
    Task<IEnumerable<Appointment>> GetAllAsync();

    /// <summary>
    /// Пребарува термини според филтрите, сортирани и странирани (10 по страница).
    /// Се користи за приказ на табелата на dashboard страницата.
    /// </summary>
    Task<IEnumerable<Appointment>> SearchAsync(AppointmentFilter filter);

    /// <summary>
    /// Го брои вкупниот број термини кои одговараат на филтрите (без пагинација).
    /// Се користи за пресметка на бројот на страници.
    /// </summary>
    Task<int> CountAsync(AppointmentFilter filter);

    /// <summary>
    /// Пресметува статистика (вкупно/закажани/завршени/откажани) според филтрите.
    /// </summary>
    Task<AppointmentStatistics> GetStatisticsAsync(AppointmentFilter filter);

    Task<Appointment?> GetByIdAsync(int id);
    Task<int> CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
    Task<bool> HasConflictAsync(int doctorId, DateTime date, TimeSpan time, int excludeId = 0);

    /// <summary>
    /// Ги менува статусот и (опционално) белешките на еден термин.
    /// Се користи за „Почеток на преглед" и „Завршување на преглед".
    /// </summary>
    Task UpdateStatusAsync(int id, string newStatus, string? notes = null);

    /// <summary>
    /// Ги враќа сите закажани времиња за конкретен доктор на конкретен датум.
    /// Се користи за пресметка на слободни термини.
    /// </summary>
    Task<IEnumerable<TimeSpan>> GetBookedTimesAsync(int doctorId, DateTime date);
}
