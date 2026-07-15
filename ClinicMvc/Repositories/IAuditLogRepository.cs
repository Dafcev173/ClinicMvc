using ClinicMvc.Models;

namespace ClinicMvc.Repositories;

public interface IAuditLogRepository
{
    Task LogAsync(string actionType, string entityName, int entityId, string username, string? description = null);
    Task<IEnumerable<AuditLog>> GetAllAsync();
}
