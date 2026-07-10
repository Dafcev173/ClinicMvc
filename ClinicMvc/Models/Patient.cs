using System.ComponentModel.DataAnnotations;

namespace ClinicMvc.Models;

public class Patient
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Името е задолжително")]
    [Display(Name = "Име")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Презимето е задолжително")]
    [Display(Name = "Презиме")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ЕМБГ е задолжително")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "ЕМБГ мора да содржи точно 13 цифри")]
    [Display(Name = "ЕМБГ")]
    public string Embg { get; set; } = string.Empty;

    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Невалидна е-пошта")]
    [Display(Name = "Е-пошта")]
    public string? Email { get; set; }
}
