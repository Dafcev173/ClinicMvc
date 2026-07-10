using System.Data;

namespace ClinicMvc.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
