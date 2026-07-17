using ClinicMvc.Models;

namespace ClinicMvc.Services;

/// <summary>Дефинира извоз на листа термини во Excel и PDF формат.</summary>
public interface IExportService
{
    byte[] ExportAppointmentsToExcel(IEnumerable<Appointment> appointments);
    byte[] ExportAppointmentsToPdf(IEnumerable<Appointment> appointments);
}
