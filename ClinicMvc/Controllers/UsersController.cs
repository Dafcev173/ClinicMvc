using ClinicMvc.Models;
using ClinicMvc.Repositories;
using ClinicMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMvc.Controllers;

/// <summary>
/// Управување со кориснички сметки - достапно само за Administrator.
/// Овозможува преглед на сите сметки, ресетирање лозинка и бришење сметка.
/// </summary>
[Authorize(Roles = "Administrator")]
public class UsersController : Controller
{
    private readonly IUserRepository     _userRepository;
    private readonly IDoctorRepository   _doctorRepository;
    private readonly IPasswordHasher     _passwordHasher;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUser;

    public UsersController(
        IUserRepository userRepository,
        IDoctorRepository doctorRepository,
        IPasswordHasher passwordHasher,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUser)
    {
        _userRepository      = userRepository;
        _doctorRepository    = doctorRepository;
        _passwordHasher      = passwordHasher;
        _auditLogRepository  = auditLogRepository;
        _currentUser         = currentUser;
    }

    /// <summary>GET: /Users - листа на сите кориснички сметки, со ime на поврзан доктор ако постои.</summary>
    public async Task<IActionResult> Index()
    {
        var users   = await _userRepository.GetAllAsync();
        var doctors = await _doctorRepository.GetAllAsync();

        foreach (var user in users)
        {
            if (user.DoctorId.HasValue)
            {
                user.Doctor = doctors.FirstOrDefault(d => d.Id == user.DoctorId.Value);
            }
        }

        return View(users);
    }

    /// <summary>POST: /Users/ResetPassword - генерира нов BCrypt hash за корисникот.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        var user = await _userRepository.GetByIdAsync(model.UserId);
        if (user == null)
            return NotFound(new { success = false, errors = new[] { "Корисникот не постои." } });

        var newHash = _passwordHasher.HashPassword(model.NewPassword);
        await _userRepository.UpdatePasswordAsync(model.UserId, newHash, _currentUser.Username);

        await _auditLogRepository.LogAsync("UPDATE", "User", model.UserId, _currentUser.Username,
            $"Ресетирана лозинка за корисник '{user.Username}'");

        return Ok(new { success = true });
    }

    /// <summary>POST: /Users/Delete/5 - трајно ја брише корисничката сметка (не влијае на медицинската историја).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { success = false, errors = new[] { "Корисникот не постои." } });

        await _userRepository.DeleteAsync(id);

        await _auditLogRepository.LogAsync("DELETE", "User", id, _currentUser.Username,
            $"Избришана корисничка сметка '{user.Username}'");

        return Ok(new { success = true });
    }
}
