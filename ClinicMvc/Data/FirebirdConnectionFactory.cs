using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace ClinicMvc.Data;

public class FirebirdConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public FirebirdConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("FirebirdConnection")
            ?? throw new InvalidOperationException(
                "Конекциониот стринг 'FirebirdConnection' не е дефиниран во appsettings.json.");
    }

    public IDbConnection CreateConnection()
    {
        var connection = new FbConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
