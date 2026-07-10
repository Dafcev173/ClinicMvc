using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task<int> CreateAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(int id);
    Task<bool> EmbgExistsAsync(string embg, int excludeId = 0);
}
