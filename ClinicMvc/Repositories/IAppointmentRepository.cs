using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> SearchAsync(AppointmentFilter filter);
    Task<int> CountAsync(AppointmentFilter filter);
    Task<AppointmentStatistics> GetStatisticsAsync(AppointmentFilter filter);
    Task<Appointment?> GetByIdAsync(int id);
    Task<int> CreateAsync(Appointment appointment, string createdBy);
    Task UpdateAsync(Appointment appointment, string modifiedBy);
    Task DeleteAsync(int id, string modifiedBy);
    Task<bool> HasConflictAsync(int doctorId, DateTime date, TimeSpan time, int excludeId = 0);
    Task UpdateStatusAsync(int id, string newStatus, string modifiedBy, string? notes = null);
    Task<IEnumerable<TimeSpan>> GetBookedTimesAsync(int doctorId, DateTime date);
}
