using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task<int> CreateAsync(Patient patient, string createdBy);
    Task UpdateAsync(Patient patient, string modifiedBy);
    Task DeleteAsync(int id, string modifiedBy);
    Task<bool> EmbgExistsAsync(string embg, int excludeId = 0);

    /// <summary>
    /// Ги враќа сите претходни термини/прегледи за конкретен пациент - медицинска историја.
    /// </summary>
    Task<IEnumerable<Appointment>> GetMedicalHistoryAsync(int patientId);
}
