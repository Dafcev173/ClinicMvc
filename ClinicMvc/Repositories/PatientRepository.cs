using ClinicMvc.Data;
using ClinicMvc.Models;
using Dapper;

namespace ClinicMvc.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PatientRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT ID, FIRSTNAME, LASTNAME, EMBG, PHONE, EMAIL
                              FROM PATIENTS
                              ORDER BY LASTNAME, FIRSTNAME";
        return await connection.QueryAsync<Patient>(sql);
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT ID, FIRSTNAME, LASTNAME, EMBG, PHONE, EMAIL
                              FROM PATIENTS WHERE ID = @Id";
        return await connection.QueryFirstOrDefaultAsync<Patient>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Patient patient)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"INSERT INTO PATIENTS (FIRSTNAME, LASTNAME, EMBG, PHONE, EMAIL)
                              VALUES (@FirstName, @LastName, @Embg, @Phone, @Email)
                              RETURNING ID";
        return await connection.ExecuteScalarAsync<int>(sql, patient);
    }

    public async Task UpdateAsync(Patient patient)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"UPDATE PATIENTS SET
                                FIRSTNAME = @FirstName,
                                LASTNAME  = @LastName,
                                EMBG      = @Embg,
                                PHONE     = @Phone,
                                EMAIL     = @Email
                              WHERE ID = @Id";
        await connection.ExecuteAsync(sql, patient);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM PATIENTS WHERE ID = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<bool> EmbgExistsAsync(string embg, int excludeId = 0)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT COUNT(*) FROM PATIENTS
                              WHERE EMBG = @Embg AND ID <> @ExcludeId";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Embg = embg, ExcludeId = excludeId });
        return count > 0;
    }
}
