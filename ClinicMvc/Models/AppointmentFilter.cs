using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicMvc.Models;

public class AppointmentFilter
{
    public int? DoctorId { get; set; }
    public string? Specialty { get; set; }
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class AppointmentIndexViewModel
{
    public AppointmentFilter Filter { get; set; } = new();
    public IEnumerable<Appointment> Appointments { get; set; } = Enumerable.Empty<Appointment>();

    public List<SelectListItem> Doctors { get; set; } = new();
    public List<SelectListItem> Specialties { get; set; } = new();
    public List<SelectListItem> Statuses { get; set; } = new();
}
