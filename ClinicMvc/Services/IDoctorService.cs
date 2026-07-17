using ClinicMvc.Models;

namespace ClinicMvc.Services;

/// <summary>
/// Дефинира бизнис логика за доктори, вклучувајќи го комбинираното
/// креирање на доктор заедно со неговата корисничка сметка.
/// </summary>
public interface IDoctorService
{
    Task<(bool Success, IEnumerable<string> Errors)> CreateDoctorWithAccountAsync(DoctorCreateViewModel model, string createdBy);
    Task<(bool Success, IEnumerable<string> Errors)> UpdateDoctorAsync(Doctor doctor, string modifiedBy);
    Task DeleteDoctorAsync(int id, string modifiedBy);
}
