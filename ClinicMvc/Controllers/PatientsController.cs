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
}
