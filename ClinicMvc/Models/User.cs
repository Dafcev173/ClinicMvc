using System.ComponentModel.DataAnnotations;

namespace ClinicMvc.Models;

/// <summary>
/// Модел за корисник (Administrator или Doctor) - одговара на табелата USERS.
/// Се користи за автентикација и авторизација.
/// </summary>
public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Корисничкото ime е задолжително")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt хеш на лозинката - НИКОГАШ не се чува plain-text лозинка.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Улога: "Administrator" или "Doctor" (CHECK constraint во базата)</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// NULL за администратори. Поставено за докторски сметки - врска кон DOCTORS.ID.
    /// </summary>
    public int? DoctorId { get; set; }

    // Audit полиња
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Навигационо поле - целосните податоци за поврзаниот доктор.
    /// Не се вчитува автоматски преку Dapper - го полни репозиториумот рачно по потреба.
    /// </summary>
    public Doctor? Doctor { get; set; }
}
