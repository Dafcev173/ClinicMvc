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
                              ORDER BY LASTNAME";
        return await connection.QueryAsync<Patient>(sql);
    }

    public async Task<int> CreateAsync(Patient patient)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"INSERT INTO PATIENTS (FIRSTNAME, LASTNAME, EMBG, PHONE, EMAIL)
                              VALUES (@FirstName, @LastName, @Embg, @Phone, @Email)
                              RETURNING ID";
        return await connection.ExecuteScalarAsync<int>(sql, patient);
    }
}
