using ClinicMvc.Data;
using ClinicMvc.Models;
using Dapper;

namespace ClinicMvc.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DoctorRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT ID, FIRSTNAME, LASTNAME, SPECIALTY, PHONE, ISACTIVE
                              FROM DOCTORS
                              ORDER BY LASTNAME";
        return await connection.QueryAsync<Doctor>(sql);
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT ID, FIRSTNAME, LASTNAME, SPECIALTY, PHONE, ISACTIVE
                              FROM DOCTORS
                              WHERE ID = @Id";
        return await connection.QueryFirstOrDefaultAsync<Doctor>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Doctor doctor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"INSERT INTO DOCTORS (FIRSTNAME, LASTNAME, SPECIALTY, PHONE, ISACTIVE)
                              VALUES (@FirstName, @LastName, @Specialty, @Phone, @IsActive)
                              RETURNING ID";
        return await connection.ExecuteScalarAsync<int>(sql, doctor);
    }

    public async Task UpdateAsync(Doctor doctor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"UPDATE DOCTORS
                              SET FIRSTNAME = @FirstName,
                                  LASTNAME = @LastName,
                                  SPECIALTY = @Specialty,
                                  PHONE = @Phone,
                                  ISACTIVE = @IsActive
                              WHERE ID = @Id";
        await connection.ExecuteAsync(sql, doctor);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM DOCTORS WHERE ID = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<IEnumerable<string>> GetSpecialtiesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT DISTINCT SPECIALTY
                              FROM DOCTORS
                              WHERE SPECIALTY IS NOT NULL
                              ORDER BY SPECIALTY";
        return await connection.QueryAsync<string>(sql);
    }
}
