using System.Text;
using ClinicMvc.Data;
using ClinicMvc.Models;
using Dapper;

namespace ClinicMvc.Repositories;

/// <summary>
/// Репозиториум за управување со термини во базата на податоци.
/// Содржи сите SQL операции поврзани со табелата APPOINTMENTS,
/// вклучувајќи пребарување со филтри, сортирање, пагинација и статистика.
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    // Фабрика за креирање на конекција со Firebird базата
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Основен FROM/JOIN дел кој се употребува во сите SELECT прашања.
    /// Забелешка: колоната PARIENTID е типографска грешка во базата (наместо PATIENTID),
    /// затоа секогаш ја алиасираме до PATIENTID за да одговара на C# моделот.
    /// </summary>
    private const string FromJoinSql = @"
        FROM APPOINTMENTS a
        JOIN DOCTORS  d ON d.ID = a.DOCTORID
        JOIN PATIENTS p ON p.ID = a.PARIENTID";

    public AppointmentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Ги враќа сите термини без филтри (интерна употреба, без пагинација).
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                a.ID, a.DOCTORID, a.PARIENTID AS PATIENTID,
                a.APPOINTMENTDATE, a.APPOINTMENTTIME, a.STATUS, a.NOTES,
                (d.FIRSTNAME || ' ' || d.LASTNAME) AS DOCTORNAME,
                (p.FIRSTNAME || ' ' || p.LASTNAME) AS PATIENTNAME,
                d.SPECIALTY AS DOCTORSPECIALTY
            " + FromJoinSql + @"
            ORDER BY a.APPOINTMENTDATE, a.APPOINTMENTTIME";
        return await connection.QueryAsync<Appointment>(sql);
    }

    /// <summary>
    /// Гради ги WHERE условите и параметрите заеднички за SearchAsync, CountAsync и GetStatisticsAsync,
    /// за да не се повторува иста логика на три места.
    /// </summary>
    private static (string WhereClause, DynamicParameters Parameters) BuildWhereClause(AppointmentFilter filter)
    {
        var sb = new StringBuilder("WHERE 1=1");
        var p  = new DynamicParameters();

        // Пребарување по Ime на пациент (делумно совпаѓање, без разлика на големина на букви)
        if (!string.IsNullOrWhiteSpace(filter.PatientFirstName))
        {
            sb.Append(" AND UPPER(p.FIRSTNAME) LIKE UPPER(@PatientFirstName)");
            p.Add("PatientFirstName", $"%{filter.PatientFirstName.Trim()}%");
        }

        // Пребарување по Презиме на пациент
        if (!string.IsNullOrWhiteSpace(filter.PatientLastName))
        {
            sb.Append(" AND UPPER(p.LASTNAME) LIKE UPPER(@PatientLastName)");
            p.Add("PatientLastName", $"%{filter.PatientLastName.Trim()}%");
        }

        // Пребарување по ЕМБГ на пациент
        if (!string.IsNullOrWhiteSpace(filter.PatientEmbg))
        {
            sb.Append(" AND p.EMBG LIKE @PatientEmbg");
            p.Add("PatientEmbg", $"%{filter.PatientEmbg.Trim()}%");
        }

        // Пребарување по Ime на лекар (проверува во целото Ime + Презиме заедно)
        if (!string.IsNullOrWhiteSpace(filter.DoctorName))
        {
            sb.Append(" AND UPPER(d.FIRSTNAME || ' ' || d.LASTNAME) LIKE UPPER(@DoctorName)");
            p.Add("DoctorName", $"%{filter.DoctorName.Trim()}%");
        }

        // Филтер по специјалност (точно совпаѓање - доаѓа од dropdown)
        if (!string.IsNullOrWhiteSpace(filter.Specialty))
        {
            sb.Append(" AND d.SPECIALTY = @Specialty");
            p.Add("Specialty", filter.Specialty);
        }

        // Филтер по конкретен датум
        if (filter.Date.HasValue)
        {
            sb.Append(" AND a.APPOINTMENTDATE = @Date");
            p.Add("Date", filter.Date.Value.Date);
        }

        return (sb.ToString(), p);
    }

    /// <summary>
    /// Ја мапира вредноста на SortBy до вистинска SQL колона.
    /// Ова е белата листа (whitelist) која спречува SQL injection преку сортирањето,
    /// бидејќи вредноста директно се вметнува во SQL текстот (не преку параметар).
    /// </summary>
    private static string GetOrderByColumn(string? sortBy) => sortBy?.ToLowerInvariant() switch
    {
        "time"    => "a.APPOINTMENTTIME",
        "patient" => "p.LASTNAME, p.FIRSTNAME",
        "doctor"  => "d.LASTNAME, d.FIRSTNAME",
        "status"  => "a.STATUS",
        _         => "a.APPOINTMENTDATE"   // "date" или непознато - користи датум по default
    };

    /// <summary>
    /// Пребарува термини според филтрите, ги сортира и ги странира (10 по страница).
    /// </summary>
    public async Task<IEnumerable<Appointment>> SearchAsync(AppointmentFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        var (where, p) = BuildWhereClause(filter);

        // Колона и насока за сортирање - direction се ограничува само на ASC/DESC (whitelist)
        var orderColumn = GetOrderByColumn(filter.SortBy);
        var direction    = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "DESC" : "ASC";

        // Пресметка на пагинација - SKIP прескокнува претходните страници
        var page = filter.Page < 1 ? 1 : filter.Page;
        var skip = (page - 1) * AppointmentFilter.PageSize;
        p.Add("PageSize", AppointmentFilter.PageSize);
        p.Add("Skip", skip);

        // Firebird синтакса: FIRST/SKIP оди веднаш по SELECT, пред листата на колони
        var sql = $@"
            SELECT FIRST @PageSize SKIP @Skip
                a.ID, a.DOCTORID, a.PARIENTID AS PATIENTID,
                a.APPOINTMENTDATE, a.APPOINTMENTTIME, a.STATUS, a.NOTES,
                (d.FIRSTNAME || ' ' || d.LASTNAME) AS DOCTORNAME,
                (p.FIRSTNAME || ' ' || p.LASTNAME) AS PATIENTNAME,
                d.SPECIALTY AS DOCTORSPECIALTY
            {FromJoinSql}
            {where}
            ORDER BY {orderColumn} {direction}";

        return await connection.QueryAsync<Appointment>(sql, p);
    }

    /// <summary>
    /// Го брои вкупниот број термини кои одговараат на филтрите (без пагинација).
    /// Се користи за пресметка на бројот на страници во пагинацијата.
    /// </summary>
    public async Task<int> CountAsync(AppointmentFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        var (where, p) = BuildWhereClause(filter);

        var sql = $@"SELECT COUNT(*) {FromJoinSql} {where}";
        return await connection.ExecuteScalarAsync<int>(sql, p);
    }

    /// <summary>
    /// Пресметува статистика (вкупно, закажани, завршени, откажани) според тековните филтри.
    /// COALESCE(...,0) е потребен затоа што SUM врз празен резултат враќа NULL во SQL.
    /// </summary>
    public async Task<AppointmentStatistics> GetStatisticsAsync(AppointmentFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        var (where, p) = BuildWhereClause(filter);

        var sql = $@"
            SELECT
                COUNT(*) AS TOTAL,
                COALESCE(SUM(CASE WHEN a.STATUS = 'Zakazan' THEN 1 ELSE 0 END), 0) AS SCHEDULED,
                COALESCE(SUM(CASE WHEN a.STATUS = 'Zavrsen' THEN 1 ELSE 0 END), 0) AS COMPLETED,
                COALESCE(SUM(CASE WHEN a.STATUS = 'Otkazen' THEN 1 ELSE 0 END), 0) AS CANCELLED
            {FromJoinSql}
            {where}";

        var stats = await connection.QueryFirstOrDefaultAsync<AppointmentStatistics>(sql, p);
        return stats ?? new AppointmentStatistics();
    }

    /// <summary>
    /// Го враќа еден термин според ID.
    /// Се користи за полнење на Edit модалот.
    /// </summary>
    public async Task<Appointment?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT ID, DOCTORID, PARIENTID AS PATIENTID,
                                     APPOINTMENTDATE, APPOINTMENTTIME, STATUS, NOTES
                              FROM APPOINTMENTS WHERE ID = @Id";
        return await connection.QueryFirstOrDefaultAsync<Appointment>(sql, new { Id = id });
    }

    /// <summary>
    /// Креира нов термин во базата.
    /// Го враќа ID-то на новиот запис (RETURNING ID).
    /// </summary>
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

    /// <summary>
    /// Ажурира постоечки термин во базата.
    /// </summary>
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

    /// <summary>
    /// Брише термин од базата според ID.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM APPOINTMENTS WHERE ID = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    /// <summary>
    /// Проверува дали докторот веќе има термин во истото датум и време.
    /// excludeId - ID на терминот кој се игнорира (при измена на постоечки термин)
    /// </summary>
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

    /// <summary>
    /// Го менува статусот на термин (пр. Zakazan → Vo tek → Zavrsen).
    /// Ако се проследат белешки, ги ажурира и нив; COALESCE го задржува старото ако е null.
    /// </summary>
    public async Task UpdateStatusAsync(int id, string newStatus, string? notes = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"UPDATE APPOINTMENTS
                              SET STATUS = @Status,
                                  NOTES  = COALESCE(@Notes, NOTES)
                              WHERE ID = @Id";
        await connection.ExecuteAsync(sql, new { Id = id, Status = newStatus, Notes = notes });
    }

    /// <summary>
    /// Ги враќа сите закажани времиња за конкретен доктор на конкретен датум.
    /// Откажаните термини (Otkazen) НЕ се сметаат за зафатени.
    /// </summary>
    public async Task<IEnumerable<TimeSpan>> GetBookedTimesAsync(int doctorId, DateTime date)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"SELECT APPOINTMENTTIME
                              FROM APPOINTMENTS
                              WHERE DOCTORID        = @DoctorId
                                AND APPOINTMENTDATE = @Date
                                AND STATUS          <> 'Otkazen'";
        return await connection.QueryAsync<TimeSpan>(sql, new { DoctorId = doctorId, Date = date.Date });
    }
}
