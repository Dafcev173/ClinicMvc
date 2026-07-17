using System.ComponentModel.DataAnnotations;

namespace ClinicMvc.Models;

/// <summary>ViewModel за ресетирање лозинка на постоечка корисничка сметка.</summary>
public class ResetPasswordViewModel
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Лозинката е задолжителна")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Лозинката мора да има најмалку 6 карактери")]
    public string NewPassword { get; set; } = string.Empty;
}
