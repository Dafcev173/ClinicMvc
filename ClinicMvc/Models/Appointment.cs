using System.ComponentModel.DataAnnotations;

namespace ClinicMvc.Models;

/// <summary>
/// Модел кој ги претставува податоците за термин.
/// Одговара на табелата APPOINTMENTS во Firebird базата.
/// </summary>
public class Appointment
{
    public int Id { get; set; }

    [Display(Name = "Лекар")]
    public int DoctorId { get; set; }

    [Display(Name = "Пациент")]
    public int PatientId { get; set; }

    [Display(Name = "Датум")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; }

    [Display(Name = "Време")]
    [DataType(DataType.Time)]
    public TimeSpan AppointmentTime { get; set; }

    /// <summary>Дозволени вредности: Zakazan, Vo tek, Zavrsen, Otkazen (CHECK constraint)</summary>
    [Display(Name = "Статус")]
    public string Status { get; set; } = "Zakazan";

    [Display(Name = "Белешки")]
    public string? Notes { get; set; }

    /// <summary>Soft delete - true значи терминот е "избришан" но записот сепак постои</summary>
    public bool IsDeleted { get; set; } = false;

    // Audit полиња
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // Пополнети преку JOIN во репозиторито - само за приказ, не се зачувуваат во базата
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorSpecialty { get; set; }
}
