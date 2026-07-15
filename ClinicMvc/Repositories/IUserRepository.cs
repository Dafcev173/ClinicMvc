using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

/// <summary>
/// Интерфејс за репозиториумот на корисници (за автентикација и авторизација).
/// </summary>
public interface IUserRepository
{
    /// <summary>Го враќа корисникот според корисничко ime - се користи при најава</summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>Го враќа корисникот според DoctorId - се користи за поврзување Doctor→User</summary>
    Task<User?> GetByDoctorIdAsync(int doctorId);

    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<int> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
