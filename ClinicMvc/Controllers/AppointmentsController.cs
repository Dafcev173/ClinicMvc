using ClinicMvc.Models;
using ClinicMvc.Repositories;
using ClinicMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicMvc.Controllers;

/// <summary>
/// Контролер за управување со термини.
/// Достапен за Administrator (гледа сè) и Doctor (гледа само свои термини -
/// рестрикцијата се применува автоматски преку ApplyDoctorRestriction()).
/// </summary>
[Authorize(Roles = "Administrator,Doctor")]
public class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository      _doctorRepository;
    private readonly IPatientRepository     _patientRepository;
    private readonly IAuditLogRepository    _auditLogRepository;
    private readonly ICurrentUserService    _currentUser;

    private static readonly string[] StatusOptions =
        { "Zakazan", "Vo tek", "Zavrsen", "Otkazen" };

    public AppointmentsController(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository      doctorRepository,
        IPatientRepository     patientRepository,
        IAuditLogRepository    auditLogRepository,
        ICurrentUserService    currentUser)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository      = doctorRepository;
        _patientRepository     = patientRepository;
        _auditLogRepository    = auditLogRepository;
        _currentUser           = currentUser;
    }

    /// <summary>
    /// Ако најавениот корисник е Doctor - го поставува RestrictToDoctorId
    /// за да гледа само свои термини. Администраторите не се ограничени.
    /// Оваа проверка е СЕРВЕРСКА - не зависи од UI, значи не може да се заобиколи.
    /// </summary>
    private void ApplyDoctorRestriction(AppointmentFilter filter)
    {
        if (_currentUser.IsDoctor && _currentUser.DoctorId.HasValue)
        {
            filter.RestrictToDoctorId = _currentUser.DoctorId.Value;
        }
    }

    public async Task<IActionResult> Index()
    {
        var specialties = await _doctorRepository.GetSpecialtiesAsync();

        var vm = new AppointmentIndexViewModel
        {
            Filter      = new AppointmentFilter(),
            Specialties = specialties.Select(s => new SelectListItem(s, s)).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> LoadTable(AppointmentFilter filter)
    {
        ApplyDoctorRestriction(filter);
        var appointments = await _appointmentRepository.SearchAsync(filter);
        return PartialView("_AppointmentsTable", appointments);
    }

    [HttpGet]
    public async Task<IActionResult> LoadPagination(AppointmentFilter filter)
    {
        ApplyDoctorRestriction(filter);
        var totalCount = await _appointmentRepository.CountAsync(filter);
        var currentPage = filter.Page < 1 ? 1 : filter.Page;
        var totalPages  = (int)Math.Ceiling(totalCount / (double)AppointmentFilter.PageSize);

        var vm = new PaginationInfo
        {
            CurrentPage = currentPage,
            TotalPages  = totalPages,
            TotalCount  = totalCount,
            PageSize    = AppointmentFilter.PageSize
        };

        return PartialView("_Pagination", vm);
    }

    [HttpGet]
    public async Task<IActionResult> LoadStatistics(AppointmentFilter filter)
    {
        ApplyDoctorRestriction(filter);
        var stats = await _appointmentRepository.GetStatisticsAsync(filter);
        return PartialView("_Statistics", stats);
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null) return NotFound();

        // Доктор смее да гледа само сопствени термини (дури и преку директен ID повик)
        if (_currentUser.IsDoctor && appointment.DoctorId != _currentUser.DoctorId)
            return Forbid();

        return Json(appointment);
    }

    [HttpGet]
    public async Task<IActionResult> GetDropdowns()
    {
        var allDoctors = await _doctorRepository.GetAllAsync();
        var patients   = await _patientRepository.GetAllAsync();
        var activeDoctors = allDoctors.Where(d => d.IsActive);

        // Доктор-корисник смее да закажува термини само за себе - dropdown-от се стеснува
        if (_currentUser.IsDoctor && _currentUser.DoctorId.HasValue)
        {
            activeDoctors = activeDoctors.Where(d => d.Id == _currentUser.DoctorId.Value);
        }

        return Json(new
        {
            doctors  = activeDoctors.Select(d => new { d.Id, name = d.FullName }),
            patients = patients.Select(p => new { p.Id, name = $"{p.FirstName} {p.LastName}" }),
            statuses = StatusOptions
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        // Доктор смее да закажува само за себе - дури и ако некој манипулира со request-от
        if (_currentUser.IsDoctor && appointment.DoctorId != _currentUser.DoctorId)
            return Forbid();

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

        var newId = await _appointmentRepository.CreateAsync(appointment, _currentUser.Username);
        await _auditLogRepository.LogAsync("CREATE", "Appointment", newId, _currentUser.Username,
            $"Закажан термин за {appointment.AppointmentDate:dd.MM.yyyy} {appointment.AppointmentTime}");

        return Ok(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Appointment appointment)
    {
        if (_currentUser.IsDoctor && appointment.DoctorId != _currentUser.DoctorId)
            return Forbid();

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

        await _appointmentRepository.UpdateAsync(appointment, _currentUser.Username);
        await _auditLogRepository.LogAsync("UPDATE", "Appointment", appointment.Id, _currentUser.Username,
            "Изменет термин");

        return Ok(new { success = true });
    }

    /// <summary>Само Administrator смее да брише термини.</summary>
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _appointmentRepository.DeleteAsync(id, _currentUser.Username);
        await _auditLogRepository.LogAsync("DELETE", "Appointment", id, _currentUser.Username, "Soft delete на термин");
        return Ok(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartExam(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return NotFound(new { success = false, errors = new[] { "Терминот не постои." } });

        if (_currentUser.IsDoctor && appointment.DoctorId != _currentUser.DoctorId)
            return Forbid();

        if (appointment.Status != "Zakazan")
            return BadRequest(new { success = false, errors = new[] { "Прегледот може да започне само за закажани термини." } });

        await _appointmentRepository.UpdateStatusAsync(id, "Vo tek", _currentUser.Username);
        await _auditLogRepository.LogAsync("UPDATE", "Appointment", id, _currentUser.Username, "Почеток на преглед");

        return Ok(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinishExam(int id, string? notes)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return NotFound(new { success = false, errors = new[] { "Терминот не постои." } });

        if (_currentUser.IsDoctor && appointment.DoctorId != _currentUser.DoctorId)
            return Forbid();

        if (appointment.Status != "Vo tek")
            return BadRequest(new { success = false, errors = new[] { "Прегледот може да заврши само ако е во тек." } });

        await _appointmentRepository.UpdateStatusAsync(id, "Zavrsen", _currentUser.Username, notes);
        await _auditLogRepository.LogAsync("UPDATE", "Appointment", id, _currentUser.Username, "Завршување на преглед");

        return Ok(new { success = true });
    }
}
