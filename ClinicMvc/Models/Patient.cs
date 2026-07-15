using System.ComponentModel.DataAnnotations;

namespace ClinicMvc.Models;

/// <summary>
/// Модел кој ги претставува податоците за пациент.
/// Одговара на табелата PATIENTS во Firebird базата.
/// </summary>
public class Patient
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Името е задолжително")]
    [Display(Name = "Ime")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Презимето е задолжително")]
    [Display(Name = "Презиме")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ЕМБГ е задолжителен")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "ЕМБГ мора да содржи точно 13 цифри")]
    [Display(Name = "ЕМБГ")]
    public string Embg { get; set; } = string.Empty;

    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Внесете валидна е-пошта адреса")]
    [Display(Name = "Е-пошта")]
    public string? Email { get; set; }

    /// <summary>Soft delete - true значи пациентот е "избришан" но записот сепак постои</summary>
    public bool IsDeleted { get; set; } = false;

    // Audit полиња
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
