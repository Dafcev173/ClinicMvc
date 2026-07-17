using ClinicMvc.Models;
using ClinicMvc.Repositories;

namespace ClinicMvc.Services;

/// <summary>
/// Бизнис логика за доктори - го координира креирањето на доктор
/// заедно со неговата корисничка сметка (два репозиториума во еден чекор).
/// </summary>
public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository   _doctorRepository;
    private readonly IUserRepository     _userRepository;
    private readonly IPasswordHasher     _passwordHasher;
    private readonly IAuditLogRepository _auditLogRepository;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuditLogRepository auditLogRepository)
    {
        _doctorRepository   = doctorRepository;
        _userRepository     = userRepository;
        _passwordHasher     = passwordHasher;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> CreateDoctorWithAccountAsync(DoctorCreateViewModel model, string createdBy)
    {
        var errors = new List<string>();

        var existingUser = await _userRepository.GetByUsernameAsync(model.Username);
        if (existingUser != null)
            errors.Add("Ова корисничко ime веќе постои.");

        if (errors.Count > 0)
            return (false, errors);

        var doctor = new Doctor
        {
            FirstName = model.FirstName,
            LastName  = model.LastName,
            Specialty = model.Specialty,
            Phone     = model.Phone,
            IsActive  = model.IsActive
        };
        var doctorId = await _doctorRepository.CreateAsync(doctor, createdBy);

        var passwordHash = _passwordHasher.HashPassword(model.Password);

        var user = new User
        {
            Username     = model.Username,
            PasswordHash = passwordHash,
            Role         = "Doctor",
            DoctorId     = doctorId,
            CreatedBy    = createdBy
        };
        await _userRepository.CreateAsync(user);

        await _auditLogRepository.LogAsync("CREATE", "Doctor", doctorId, createdBy,
            $"Креиран доктор {doctor.FirstName} {doctor.LastName}");
        await _auditLogRepository.LogAsync("CREATE", "User", doctorId, createdBy,
            $"Креирана корисничка сметка '{model.Username}' за доктор {doctor.FirstName} {doctor.LastName}");

        return (true, errors);
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> UpdateDoctorAsync(Doctor doctor, string modifiedBy)
    {
        await _doctorRepository.UpdateAsync(doctor, modifiedBy);
        await _auditLogRepository.LogAsync("UPDATE", "Doctor", doctor.Id, modifiedBy,
            $"Изменет доктор {doctor.FirstName} {doctor.LastName}");

        return (true, Enumerable.Empty<string>());
    }

    public async Task DeleteDoctorAsync(int id, string modifiedBy)
    {
        await _doctorRepository.DeleteAsync(id, modifiedBy);
        await _auditLogRepository.LogAsync("DELETE", "Doctor", id, modifiedBy, "Soft delete на доктор");
    }
}
