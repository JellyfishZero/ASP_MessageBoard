using System.Data.Common;

namespace ASP_MessageBoard.Repositories
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
