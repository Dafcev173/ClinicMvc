using ClinicMvc.Models;
using ClinicMvc.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicMvc.Controllers;

/// <summary>
/// Контролер за управување со термини.
/// Ги обработува CRUD операциите, статус-преодите, и AJAX барањата за
/// dashboard страницата (табела, статистика, сортирање, пагинација).
/// </summary>
public class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository      _doctorRepository;
    private readonly IPatientRepository     _patientRepository;

    // Дозволени вредности за статусот на термин - мора да се совпаѓаат со CHECK constraint во базата
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

    /// <summary>
    /// GET: /Appointments
    /// Ја вчитува dashboard страницата со филтри.
    /// Табелата, статистиката и пагинацијата се вчитуваат посебно преку AJAX.
    /// </summary>
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

    /// <summary>
    /// GET: /Appointments/LoadTable
    /// AJAX endpoint кој враќа само Partial View со табелата на термини
    /// (веќе филтрирана, сортирана и странирана).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LoadTable(AppointmentFilter filter)
    {
        var appointments = await _appointmentRepository.SearchAsync(filter);
        return PartialView("_AppointmentsTable", appointments);
    }

    /// <summary>
    /// GET: /Appointments/LoadPagination
    /// AJAX endpoint кој го брои вкупниот број записи според филтрите
    /// и враќа Partial View со Previous/Next контролите.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LoadPagination(AppointmentFilter filter)
    {
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

    /// <summary>
    /// GET: /Appointments/LoadStatistics
    /// AJAX endpoint кој ја пресметува статистиката според тековните филтри
    /// (игнорирајќи сортирање и страница) и враќа Partial View со карти.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LoadStatistics(AppointmentFilter filter)
    {
        var stats = await _appointmentRepository.GetStatisticsAsync(filter);
        return PartialView("_Statistics", stats);
    }

    /// <summary>
    /// GET: /Appointments/GetById/5
    /// Враќа JSON со податоците за еден термин.
    /// Се користи за полнење на Edit модалот преку AJAX.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null) return NotFound();
        return Json(appointment);
    }

    /// <summary>
    /// GET: /Appointments/GetDropdowns
    /// Враќа JSON со листи за dropdown-ите во Create/Edit модалот.
    /// Само активни лекари може да добијат нови термини.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDropdowns()
    {
        var allDoctors = await _doctorRepository.GetAllAsync();
        var patients   = await _patientRepository.GetAllAsync();
        var activeDoctors = allDoctors.Where(d => d.IsActive);

        return Json(new
        {
            doctors  = activeDoctors.Select(d => new { d.Id, name = d.FullName }),
            patients = patients.Select(p => new { p.Id, name = $"{p.FirstName} {p.LastName}" }),
            statuses = StatusOptions
        });
    }

    /// <summary>
    /// POST: /Appointments/Create
    /// Креира нов термин по успешна валидација.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
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

    /// <summary>
    /// POST: /Appointments/Edit
    /// Ажурира постоечки термин по успешна валидација.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Appointment appointment)
    {
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

    /// <summary>
    /// POST: /Appointments/Delete
    /// Брише термин според ID.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _appointmentRepository.DeleteAsync(id);
        return Ok(new { success = true });
    }

    /// <summary>
    /// POST: /Appointments/StartExam
    /// Task 8 - Почеток на преглед: Zakazan → Vo tek.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartExam(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return NotFound(new { success = false, errors = new[] { "Терминот не постои." } });

        if (appointment.Status != "Zakazan")
            return BadRequest(new { success = false, errors = new[] { "Прегледот може да започне само за закажани термини." } });

        await _appointmentRepository.UpdateStatusAsync(id, "Vo tek");
        return Ok(new { success = true });
    }

    /// <summary>
    /// POST: /Appointments/FinishExam
    /// Task 9 - Завршување на преглед: Vo tek → Zavrsen, со белешки.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinishExam(int id, string? notes)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return NotFound(new { success = false, errors = new[] { "Терминот не постои." } });

        if (appointment.Status != "Vo tek")
            return BadRequest(new { success = false, errors = new[] { "Прегледот може да заврши само ако е во тек." } });

        await _appointmentRepository.UpdateStatusAsync(id, "Zavrsen", notes);
        return Ok(new { success = true });
    }
}
