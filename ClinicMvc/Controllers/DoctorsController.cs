using ClinicMvc.Models;
using ClinicMvc.Repositories;
using ClinicMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

/// <summary>
/// Контролер за управување со доктори.
/// [Authorize(Roles = "Administrator")] на класата значи ЦЕЛИОТ контролер
/// е достапен само за администратори - докторите не смеат да додаваат/бришат доктори.
/// </summary>
[Authorize(Roles = "Administrator")]
public class DoctorsController : Controller
{
    private readonly IDoctorRepository    _doctorRepository;
    private readonly IAuditLogRepository  _auditLogRepository;
    private readonly ICurrentUserService  _currentUser;

    public DoctorsController(
        IDoctorRepository doctorRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUser)
    {
        _doctorRepository   = doctorRepository;
        _auditLogRepository = auditLogRepository;
        _currentUser        = currentUser;
    }

    /// <summary>GET: /Doctors - листа со Ime, Презиме, Специјалност, Акции.</summary>
    public async Task<IActionResult> Index()
    {
        var doctors = await _doctorRepository.GetAllAsync();
        return View(doctors);
    }

    /// <summary>
    /// GET: /Doctors/Details/5
    /// Детали за доктор + денешен распоред (Дел 4 од барањето).
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null) return NotFound();

        var todaySchedule = await _doctorRepository.GetTodayScheduleAsync(id);
        ViewBag.TodaySchedule = todaySchedule;

        return View(doctor);
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null) return NotFound();
        return Json(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Doctor doctor)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        var newId = await _doctorRepository.CreateAsync(doctor, _currentUser.Username);

        // Автоматско логирање во AuditLogs (Дел 9)
        await _auditLogRepository.LogAsync("CREATE", "Doctor", newId, _currentUser.Username,
            $"Креиран доктор {doctor.FirstName} {doctor.LastName}");

        return Ok(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Doctor doctor)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        await _doctorRepository.UpdateAsync(doctor, _currentUser.Username);

        await _auditLogRepository.LogAsync("UPDATE", "Doctor", doctor.Id, _currentUser.Username,
            $"Изменет доктор {doctor.FirstName} {doctor.LastName}");

        return Ok(new { success = true });
    }

    /// <summary>
    /// POST: /Doctors/Delete/5
    /// SOFT DELETE - записот не се брише физички (Дел 7).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _doctorRepository.DeleteAsync(id, _currentUser.Username);

        await _auditLogRepository.LogAsync("DELETE", "Doctor", id, _currentUser.Username,
            "Soft delete на доктор");

        return Ok(new { success = true });
    }
}
