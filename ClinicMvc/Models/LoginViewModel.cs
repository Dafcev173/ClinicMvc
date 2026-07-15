using System.ComponentModel.DataAnnotations;

namespace ClinicMvc.Models;

/// <summary>ViewModel за формата за најава.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Корисничкото ime е задолжително")]
    [Display(Name = "Корисничко ime")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Лозинката е задолжителна")]
    [DataType(DataType.Password)]
    [Display(Name = "Лозинка")]
    public string Password { get; set; } = string.Empty;
}
