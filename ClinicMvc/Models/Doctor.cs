using System.ComponentModel.DataAnnotations;

namespace ClinicMvc.Models;

public class Doctor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Името е задолжително")]
    [Display(Name = "Име")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Презимето е задолжително")]
    [Display(Name = "Презиме")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Специјалност")]
    public string? Specialty { get; set; }

    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;
}
