using ClinicMvc.Models;
using ClinicMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicMvc.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository      _doctorRepository;
    private readonly IPatientRepository     _patientRepository;

    // Мора да се совпаѓаат со CHECK constraint во базата
    private static readonly string[] StatusOptions =
        { "Zakazan", "Vo tek", "Zavrsen", "Otkazen" };

    public AppointmentsController(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository      doctorRepository,
        IPatientRepository     patientRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository      = doctorRepository;
        _patientRepository     = patientRepository;
    }

    // GET: /Appointments
    public async Task<IActionResult> Index(AppointmentFilter filter)
    {
        var appointments = await _appointmentRepository.SearchAsync(filter);
        var doctors      = await _doctorRepository.GetAllAsync();
        var specialties  = await _doctorRepository.GetSpecialtiesAsync();

        var vm = new AppointmentIndexViewModel
        {
            Filter       = filter,
            Appointments = appointments,
            Doctors      = doctors.Select(d =>
                new SelectListItem(d.FullName, d.Id.ToString())).ToList(),
            Specialties  = specialties.Select(s =>
                new SelectListItem(s, s)).ToList(),
            Statuses     = StatusOptions.Select(s =>
                new SelectListItem(s, s)).ToList()
        };

        return View(vm);
    }

    // GET: /Appointments/GetById/5  (за Edit Modal)
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null) return NotFound();
        return Json(appointment);
    }

    // GET: /Appointments/GetDropdowns  (за полнење на Modal dropdown-и)
    [HttpGet]
    public async Task<IActionResult> GetDropdowns()
    {
        var doctors  = await _doctorRepository.GetAllAsync();
        var patients = await _patientRepository.GetAllAsync();
        return Json(new
        {
            doctors  = doctors.Select(d  => new { d.Id, name = d.FullName }),
            patients = patients.Select(p => new { p.Id, name = $"{p.FirstName} {p.LastName}" }),
            statuses = StatusOptions
        });
    }

    // POST: /Appointments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        // Не смее да биде во минатото
        if (appointment.AppointmentDate.Date < DateTime.Today)
            ModelState.AddModelError("AppointmentDate", "Не може да се закаже термин за минат датум.");

        // Конфликт — ист доктор, исто датум+време
        if (await _appointmentRepository.HasConflictAsync(
                appointment.DoctorId, appointment.AppointmentDate, appointment.AppointmentTime))
            ModelState.AddModelError("AppointmentTime",
                "Докторот веќе има термин во тоа датум и време.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        await _appointmentRepository.CreateAsync(appointment);
        return Ok(new { success = true });
    }

    // POST: /Appointments/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Appointment appointment)
    {
        // Не смее да биде во минатото
        if (appointment.AppointmentDate.Date < DateTime.Today)
            ModelState.AddModelError("AppointmentDate", "Не може да се закаже термин за минат датум.");

        // Конфликт (исклучи го тековниот термин)
        if (await _appointmentRepository.HasConflictAsync(
                appointment.DoctorId, appointment.AppointmentDate,
                appointment.AppointmentTime, appointment.Id))
            ModelState.AddModelError("AppointmentTime",
                "Докторот веќе има термин во тоа датум и време.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        await _appointmentRepository.UpdateAsync(appointment);
        return Ok(new { success = true });
    }

    // POST: /Appointments/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _appointmentRepository.DeleteAsync(id);
        return Ok(new { success = true });
    }
}
