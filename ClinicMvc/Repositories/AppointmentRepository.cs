using System.Text;
using ClinicMvc.Data;
using ClinicMvc.Models;
using Dapper;

namespace ClinicMvc.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AppointmentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await SearchAsync(new AppointmentFilter());
    }

    public async Task<IEnumerable<Appointment>> SearchAsync(AppointmentFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Забелешка: колоната во базата е именувана PARIENTID (типографска грешка
        // направена при рачно креирање на табелата), затоа ја алиасираме до PATIENTID
        // за да одговара на Appointment.PatientId во моделот.
        var sql = new StringBuilder(@"SELECT
                                a.ID,
                                a.DOCTORID,
                                a.PARIENTID AS PATIENTID,
                                a.APPOINTMENTDATE,
                                a.APPOINTMENTTIME,
                                a.STATUS,
                                a.NOTES,
                                (d.FIRSTNAME || ' ' || d.LASTNAME) AS DOCTORNAME,
                                (p.FIRSTNAME || ' ' || p.LASTNAME) AS PATIENTNAME,
                                d.SPECIALTY AS DOCTORSPECIALTY
                              FROM APPOINTMENTS a
                              JOIN DOCTORS d ON d.ID = a.DOCTORID
                              JOIN PATIENTS p ON p.ID = a.PARIENTID
                              WHERE 1 = 1");

        var parameters = new DynamicParameters();

        if (filter.DoctorId.HasValue)
        {
            sql.Append(" AND a.DOCTORID = @DoctorId");
            parameters.Add("DoctorId", filter.DoctorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Specialty))
        {
            sql.Append(" AND d.SPECIALTY = @Specialty");
            parameters.Add("Specialty", filter.Specialty);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            sql.Append(" AND a.STATUS = @Status");
            parameters.Add("Status", filter.Status);
        }

        if (filter.DateFrom.HasValue)
        {
            sql.Append(" AND a.APPOINTMENTDATE >= @DateFrom");
            parameters.Add("DateFrom", filter.DateFrom.Value.Date);
        }

        if (filter.DateTo.HasValue)
        {
            sql.Append(" AND a.APPOINTMENTDATE <= @DateTo");
            parameters.Add("DateTo", filter.DateTo.Value.Date);
        }

        sql.Append(" ORDER BY a.APPOINTMENTDATE, a.APPOINTMENTTIME");

        return await connection.QueryAsync<Appointment>(sql.ToString(), parameters);
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT
                                ID,
                                DOCTORID,
                                PARIENTID AS PATIENTID,
                                APPOINTMENTDATE,
                                APPOINTMENTTIME,
                                STATUS,
                                NOTES
                              FROM APPOINTMENTS
                              WHERE ID = @Id";
        return await connection.QueryFirstOrDefaultAsync<Appointment>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Appointment appointment)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"INSERT INTO APPOINTMENTS
                                (DOCTORID, PARIENTID, APPOINTMENTDATE, APPOINTMENTTIME, STATUS, NOTES)
                              VALUES
                                (@DoctorId, @PatientId, @AppointmentDate, @AppointmentTime, @Status, @Notes)
                              RETURNING ID";
        return await connection.ExecuteScalarAsync<int>(sql, appointment);
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"UPDATE APPOINTMENTS
                              SET DOCTORID = @DoctorId,
                                  PARIENTID = @PatientId,
                                  APPOINTMENTDATE = @AppointmentDate,
                                  APPOINTMENTTIME = @AppointmentTime,
                                  STATUS = @Status,
                                  NOTES = @Notes
                              WHERE ID = @Id";
        await connection.ExecuteAsync(sql, appointment);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM APPOINTMENTS WHERE ID = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
