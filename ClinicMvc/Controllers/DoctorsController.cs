using ClinicMvc.Models;
using ClinicMvc.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

public class DoctorsController : Controller
{
    private readonly IDoctorRepository _doctorRepository;

    public DoctorsController(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    // GET: /Doctors
    public async Task<IActionResult> Index()
    {
        var doctors = await _doctorRepository.GetAllAsync();
        return View(doctors);
    }

    // GET: /Doctors/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Doctors/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Doctor doctor)
    {
        if (!ModelState.IsValid)
        {
            return View(doctor);
        }

        await _doctorRepository.CreateAsync(doctor);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Doctors/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null)
        {
            return NotFound();
        }
        return View(doctor);
    }

    // POST: /Doctors/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Doctor doctor)
    {
        if (id != doctor.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(doctor);
        }

        await _doctorRepository.UpdateAsync(doctor);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Doctors/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor == null)
        {
            return NotFound();
        }
        return View(doctor);
    }

    // POST: /Doctors/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _doctorRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
