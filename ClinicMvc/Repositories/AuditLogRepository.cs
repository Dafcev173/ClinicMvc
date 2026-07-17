using ClinicMvc.Data;
using ClinicMvc.Models;
using Dapper;

namespace ClinicMvc.Repositories;

/// <summary>
/// Репозиториум за AuditLogs табелата.
/// LogAsync се повикува рачно по секоја успешна Create/Update/Delete операција
/// во контролерите (наместо автоматски "hook", за целосна контрола и јасност).
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditLogRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task LogAsync(string actionType, string entityName, int entityId, string username, string? description = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"INSERT INTO AUDITLOGS
                                (ACTIONTYPE, ENTITYNAME, ENTITYID, USERNAME, LOGDATETIME, DESCRIPTION)
                              VALUES
                                (@ActionType, @EntityName, @EntityId, @Username, CURRENT_TIMESTAMP, @Description)";
        await connection.ExecuteAsync(sql, new { ActionType = actionType, EntityName = entityName, EntityId = entityId, Username = username, Description = description });
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT ID, ACTIONTYPE, ENTITYNAME, ENTITYID, USERNAME, LOGDATETIME, DESCRIPTION
                              FROM AUDITLOGS
                              ORDER BY LOGDATETIME DESC";
        return await connection.QueryAsync<AuditLog>(sql);
    }

    public async Task<IEnumerable<AuditLog>> GetPagedAsync(int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();
        var validPage = page < 1 ? 1 : page;
        var skip = (validPage - 1) * pageSize;

        // Firebird синтакса: FIRST/SKIP оди веднаш по SELECT
        const string sql = @"SELECT FIRST @PageSize SKIP @Skip
                                ID, ACTIONTYPE, ENTITYNAME, ENTITYID, USERNAME, LOGDATETIME, DESCRIPTION
                              FROM AUDITLOGS
                              ORDER BY LOGDATETIME DESC";
        return await connection.QueryAsync<AuditLog>(sql, new { PageSize = pageSize, Skip = skip });
    }

    public async Task<int> CountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM AUDITLOGS";
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
