using ClinicMvc.Models;
using ClinicMvc.Repositories;
using ClinicMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

/// <summary>
/// Контролер за управување со пациенти.
/// Достапен за Administrator И Doctor (докторите гледаат пациенти заради нивните термини),
/// но Create/Edit/Delete остануваат ограничени - видете подолу.
/// </summary>
[Authorize(Roles = "Administrator,Doctor")]
public class PatientsController : Controller
{
    private readonly IPatientRepository   _patientRepository;
    private readonly IAuditLogRepository  _auditLogRepository;
    private readonly ICurrentUserService  _currentUser;

    public PatientsController(
        IPatientRepository patientRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUser)
    {
        _patientRepository  = patientRepository;
        _auditLogRepository = auditLogRepository;
        _currentUser        = currentUser;
    }

    /// <summary>GET: /Patients - листа со Ime, Презиме, ЕМБГ, Акции.</summary>
    public async Task<IActionResult> Index()
    {
        var patients = await _patientRepository.GetAllAsync();
        return View(patients);
    }

    /// <summary>
    /// GET: /Patients/Details/5
    /// Детали за пациент + медицинска историја (сите претходни термини) - Дел 6.
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null) return NotFound();

        var history = await _patientRepository.GetMedicalHistoryAsync(id);
        ViewBag.MedicalHistory = history;

        return View(patient);
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return Json(patient);
    }

    /// <summary>Само Administrator смее да додава пациенти.</summary>
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (await _patientRepository.EmbgExistsAsync(patient.Embg))
            ModelState.AddModelError("Embg", "ЕМБГ веќе постои во системот.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        var newId = await _patientRepository.CreateAsync(patient, _currentUser.Username);
        await _auditLogRepository.LogAsync("CREATE", "Patient", newId, _currentUser.Username,
            $"Креиран пациент {patient.FirstName} {patient.LastName}");

        return Ok(new { success = true });
    }

    /// <summary>Само Administrator смее да менува пациенти.</summary>
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Patient patient)
    {
        if (await _patientRepository.EmbgExistsAsync(patient.Embg, patient.Id))
            ModelState.AddModelError("Embg", "ЕМБГ веќе постои во системот.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        await _patientRepository.UpdateAsync(patient, _currentUser.Username);
        await _auditLogRepository.LogAsync("UPDATE", "Patient", patient.Id, _currentUser.Username,
            $"Изменет пациент {patient.FirstName} {patient.LastName}");

        return Ok(new { success = true });
    }

    /// <summary>Само Administrator смее да брише (soft delete) пациенти.</summary>
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientRepository.DeleteAsync(id, _currentUser.Username);
        await _auditLogRepository.LogAsync("DELETE", "Patient", id, _currentUser.Username, "Soft delete на пациент");
        return Ok(new { success = true });
    }
}
