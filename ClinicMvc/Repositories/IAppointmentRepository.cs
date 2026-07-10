using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> SearchAsync(AppointmentFilter filter);
    Task<Appointment?> GetByIdAsync(int id);
    Task<int> CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
}
