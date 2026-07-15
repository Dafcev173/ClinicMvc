namespace ClinicMvc.Services;

/// <summary>
/// Имплементација на IPasswordHasher со користење на BCrypt.Net-Next пакетот.
/// BCrypt автоматски генерира "salt" за секоја лозинка и е отпорен на brute-force напади.
///
/// ВАЖНО: треба да се додаде NuGet пакетот "BCrypt.Net-Next" во .csproj:
///   &lt;PackageReference Include="BCrypt.Net-Next" Version="4.0.3" /&gt;
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// WorkFactor го контролира бројот на "рунди" на хеширање - повисока вредност
    /// значи побезбедно, но побавно. 11 е добар баланс за веб апликации.
    /// </summary>
    private const int WorkFactor = 11;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
