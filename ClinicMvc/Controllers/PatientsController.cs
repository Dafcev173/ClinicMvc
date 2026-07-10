using ClinicMvc.Models;
using ClinicMvc.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

public class PatientsController : Controller
{
    private readonly IPatientRepository _patientRepository;

    public PatientsController(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    // GET: /Patients
    public async Task<IActionResult> Index()
    {
        var patients = await _patientRepository.GetAllAsync();
        return View(patients);
    }

    // GET: /Patients/GetById/5
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return Json(patient);
    }

    // POST: /Patients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        // EMBG уникатност
        if (await _patientRepository.EmbgExistsAsync(patient.Embg))
            ModelState.AddModelError("Embg", "ЕМБГ веќе постои во системот.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        await _patientRepository.CreateAsync(patient);
        return Ok(new { success = true });
    }

    // POST: /Patients/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Patient patient)
    {
        // EMBG уникатност (исклучи го тековниот пациент)
        if (await _patientRepository.EmbgExistsAsync(patient.Embg, patient.Id))
            ModelState.AddModelError("Embg", "ЕМБГ веќе постои во системот.");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        await _patientRepository.UpdateAsync(patient);
        return Ok(new { success = true });
    }

    // POST: /Patients/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientRepository.DeleteAsync(id);
        return Ok(new { success = true });
    }
}
