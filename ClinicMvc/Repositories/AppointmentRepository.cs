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

    // Основен SELECT со JOIN-ови за приказ
    private const string SelectSql = @"
        SELECT
            a.ID,
            a.DOCTORID,
            a.PARIENTID      AS PATIENTID,
            a.APPOINTMENTDATE,
            a.APPOINTMENTTIME,
            a.STATUS,
            a.NOTES,
            (d.FIRSTNAME || ' ' || d.LASTNAME) AS DOCTORNAME,
            (p.FIRSTNAME || ' ' || p.LASTNAME) AS PATIENTNAME,
            d.SPECIALTY AS DOCTORSPECIALTY
        FROM APPOINTMENTS a
        JOIN DOCTORS  d ON d.ID = a.DOCTORID
        JOIN PATIENTS p ON p.ID = a.PARIENTID";

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await SearchAsync(new AppointmentFilter());
    }

    public async Task<IEnumerable<Appointment>> SearchAsync(AppointmentFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = new StringBuilder(SelectSql + " WHERE 1=1");
        var p   = new DynamicParameters();

        if (filter.DoctorId.HasValue)
        {
            sql.Append(" AND a.DOCTORID = @DoctorId");
            p.Add("DoctorId", filter.DoctorId.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.Specialty))
        {
            sql.Append(" AND d.SPECIALTY = @Specialty");
            p.Add("Specialty", filter.Specialty);
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            sql.Append(" AND a.STATUS = @Status");
            p.Add("Status", filter.Status);
        }
        if (filter.DateFrom.HasValue)
        {
            sql.Append(" AND a.APPOINTMENTDATE >= @DateFrom");
            p.Add("DateFrom", filter.DateFrom.Value.Date);
        }
        if (filter.DateTo.HasValue)
        {
            sql.Append(" AND a.APPOINTMENTDATE <= @DateTo");
            p.Add("DateTo", filter.DateTo.Value.Date);
        }

        sql.Append(" ORDER BY a.APPOINTMENTDATE, a.APPOINTMENTTIME");
        return await connection.QueryAsync<Appointment>(sql.ToString(), p);
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT ID, DOCTORID, PARIENTID AS PATIENTID,
                                     APPOINTMENTDATE, APPOINTMENTTIME, STATUS, NOTES
                              FROM APPOINTMENTS WHERE ID = @Id";
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
        const string sql = @"UPDATE APPOINTMENTS SET
                                DOCTORID          = @DoctorId,
                                PARIENTID         = @PatientId,
                                APPOINTMENTDATE   = @AppointmentDate,
                                APPOINTMENTTIME   = @AppointmentTime,
                                STATUS            = @Status,
                                NOTES             = @Notes
                              WHERE ID = @Id";
        await connection.ExecuteAsync(sql, appointment);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM APPOINTMENTS WHERE ID = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    // Провери дали докторот веќе има термин во истото датум+време
    public async Task<bool> HasConflictAsync(int doctorId, DateTime date, TimeSpan time, int excludeId = 0)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT COUNT(*) FROM APPOINTMENTS
                              WHERE DOCTORID        = @DoctorId
                                AND APPOINTMENTDATE = @Date
                                AND APPOINTMENTTIME = @Time
                                AND ID             <> @ExcludeId";
        var count = await connection.ExecuteScalarAsync<int>(sql,
            new { DoctorId = doctorId, Date = date.Date, Time = time, ExcludeId = excludeId });
        return count > 0;
    }
}
