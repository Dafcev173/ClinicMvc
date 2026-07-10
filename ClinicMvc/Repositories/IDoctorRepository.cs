using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

public interface IDoctorRepository
{
    Task<IEnumerable<Doctor>> GetAllAsync();
    Task<Doctor?> GetByIdAsync(int id);
    Task<int> CreateAsync(Doctor doctor);
    Task UpdateAsync(Doctor doctor);
    Task DeleteAsync(int id);
    Task<IEnumerable<string>> GetSpecialtiesAsync();
}
