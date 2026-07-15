namespace ClinicMvc.Models;

/// <summary>
/// Модел за еден запис на CRUD активност - одговара на табелата AUDITLOGS.
/// Се генерира автоматски при секоја Create/Update/Delete операција.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    /// <summary>CREATE, UPDATE или DELETE</summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>Ime на ентитетот: Doctor, Patient, Appointment</summary>
    public string EntityName { get; set; } = string.Empty;

    public int EntityId { get; set; }

    /// <summary>Корисничко ime на лицето кое ја извршило акцијата</summary>
    public string Username { get; set; } = string.Empty;

    public DateTime LogDateTime { get; set; }

    /// <summary>Опис на промената (пр. "Изменета специјалност")</summary>
    public string? Description { get; set; }
}
