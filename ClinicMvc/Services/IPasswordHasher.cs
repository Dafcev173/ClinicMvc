namespace ClinicMvc.Services;

/// <summary>
/// Интерфејс за хеширање и проверка на лозинки.
/// НИКОГАШ не се чуваат plain-text лозинки во базата.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Ја хешира дадената лозинка - се повикува при регистрација/промена на лозинка.</summary>
    string HashPassword(string password);

    /// <summary>Проверува дали внесената лозинка одговара на зачуваниот хеш - се повикува при најава.</summary>
    bool VerifyPassword(string password, string passwordHash);
}
