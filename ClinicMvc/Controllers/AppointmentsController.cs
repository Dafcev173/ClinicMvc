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

    // GET: /Appointments — ја вчитува страната со филтри (само еднаш)
    public async Task<IActionResult> Index()
    {
        var doctors     = await _doctorRepository.GetAllAsync();
        var specialties = await _doctorRepository.GetSpecialtiesAsync();

        var vm = new AppointmentIndexViewModel
        {
            Filter      = new AppointmentFilter(),
            Appointments = Enumerable.Empty<Appointment>(),
            Doctors     = doctors.Select(d =>
                new SelectListItem(d.FullName, d.Id.ToString())).ToList(),
            Specialties = specialties.Select(s =>
                new SelectListItem(s, s)).ToList(),
            Statuses    = StatusOptions.Select(s =>
                new SelectListItem(s, s)).ToList()
        };

        return View(vm);
    }

    // GET: /Appointments/LoadTable — AJAX endpoint, враќа само Partial View
    // Се повикува при: вчитување, пребарување, по Create/Edit/Delete
    [HttpGet]
    public async Task<IActionResult> LoadTable(AppointmentFilter filter)
    {
        var appointments = await _appointmentRepository.SearchAsync(filter);
        return PartialView("_AppointmentsTable", appointments);
    }

    // GET: /Appointments/GetById/5
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null) return NotFound();
        return Json(appointment);
    }

    // GET: /Appointments/GetDropdowns
    [HttpGet]
    public async Task<IActionResult> GetDropdowns()
    {
        var allDoctors = await _doctorRepository.GetAllAsync();
        var patients   = await _patientRepository.GetAllAsync();

        // Само активни доктори може да добијат нови термини
        var activeDoctors = allDoctors.Where(d => d.IsActive);

        return Json(new
        {
            doctors  = activeDoctors.Select(d  => new { d.Id, name = d.FullName }),
            patients = patients.Select(p => new { p.Id, name = $"{p.FirstName} {p.LastName}" }),
            statuses = StatusOptions
        });
    }

    // POST: /Appointments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        // Неактивен доктор
        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);
        if (doctor == null || !doctor.IsActive)
            ModelState.AddModelError("DoctorId", "Избраниот лекар не е активен.");

        if (appointment.AppointmentDate.Date < DateTime.Today)
            ModelState.AddModelError("AppointmentDate", "Не може да се закаже термин за минат датум.");

        if (await _appointmentRepository.HasConflictAsync(
                appointment.DoctorId, appointment.AppointmentDate, appointment.AppointmentTime))
            ModelState.AddModelError("AppointmentTime", "Докторот веќе има термин во тоа датум и време.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
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
        // Неактивен доктор
        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);
        if (doctor == null || !doctor.IsActive)
            ModelState.AddModelError("DoctorId", "Избраниот лекар не е активен.");

        if (appointment.AppointmentDate.Date < DateTime.Today)
            ModelState.AddModelError("AppointmentDate", "Не може да се закаже термин за минат датум.");

        if (await _appointmentRepository.HasConflictAsync(
                appointment.DoctorId, appointment.AppointmentDate,
                appointment.AppointmentTime, appointment.Id))
            ModelState.AddModelError("AppointmentTime", "Докторот веќе има термин во тоа датум и време.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
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
