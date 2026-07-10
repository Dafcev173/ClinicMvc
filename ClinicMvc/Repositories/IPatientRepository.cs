using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<int> CreateAsync(Patient patient);
}
