using System.ComponentModel.DataAnnotations;

namespace ClinicMvc.Models;

public class Appointment
{
    public int Id { get; set; }

    [Display(Name = "Доктор")]
    public int DoctorId { get; set; }

    [Display(Name = "Пациент")]
    public int PatientId { get; set; }

    [Display(Name = "Датум")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; }

    [Display(Name = "Време")]
    [DataType(DataType.Time)]
    public TimeSpan AppointmentTime { get; set; }

    [Display(Name = "Статус")]
    public string Status { get; set; } = "Закажан";

    [Display(Name = "Белешки")]
    public string? Notes { get; set; }

    // Пополнети преку JOIN во репозиторито, само за приказ во листи
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorSpecialty { get; set; }
}
