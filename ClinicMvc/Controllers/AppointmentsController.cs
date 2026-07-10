using ClinicMvc.Models;
using ClinicMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicMvc.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;

    // ВНИМАНИЕ: овие мора да се ИДЕНТИЧНИ со вредностите во
    // CHECK constraint-от CHK_APPOINTMENTS_STATUS во базата.
    // Твојата база моментално содржи латинична транслитерација
    // ('Zakazan', 'Vo tek', 'Zavrsen', 'Otkazen') наместо кирилица.
    // Ако подоцна го смениш CHECK constraint-от, ажурирај ја и оваа листа.
    private static readonly string[] StatusOptions =
    {
        "Zakazan", "Vo tek", "Zavrsen", "Otkazen"
    };

    public AppointmentsController(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
    }

    // GET: /Appointments  (Главна страна)
    public async Task<IActionResult> Index(AppointmentFilter filter)
    {
        var appointments = await _appointmentRepository.SearchAsync(filter);
        var doctors = await _doctorRepository.GetAllAsync();
        var specialties = await _doctorRepository.GetSpecialtiesAsync();

        var viewModel = new AppointmentIndexViewModel
        {
            Filter = filter,
            Appointments = appointments,
            Doctors = doctors
                .Select(d => new SelectListItem($"{d.FirstName} {d.LastName}", d.Id.ToString()))
                .ToList(),
            Specialties = specialties
                .Select(s => new SelectListItem(s, s))
                .ToList(),
            Statuses = StatusOptions
                .Select(s => new SelectListItem(s, s))
                .ToList()
        };

        return View(viewModel);
    }

    // GET: /Appointments/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new Appointment { AppointmentDate = DateTime.Today });
    }

    // POST: /Appointments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(appointment);
        }

        await _appointmentRepository.CreateAsync(appointment);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Appointments/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }

        await PopulateDropdownsAsync();
        return View(appointment);
    }

    // POST: /Appointments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(appointment);
        }

        await _appointmentRepository.UpdateAsync(appointment);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Appointments/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }
        return View(appointment);
    }

    // POST: /Appointments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _appointmentRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync()
    {
        var doctors = await _doctorRepository.GetAllAsync();
        var patients = await _patientRepository.GetAllAsync();

        ViewBag.Doctors = doctors
            .Select(d => new SelectListItem($"{d.FirstName} {d.LastName}", d.Id.ToString()))
            .ToList();

        ViewBag.Patients = patients
            .Select(p => new SelectListItem($"{p.FirstName} {p.LastName}", p.Id.ToString()))
            .ToList();

        ViewBag.Statuses = StatusOptions
            .Select(s => new SelectListItem(s, s))
            .ToList();
    }
}
