using Microsoft.Data.SqlClient;

namespace ZadElealm.Repository.Data.Datbases;

public static class SqlServerConnectionString
{
    public static string WithoutMultipleActiveResultSets(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The DefaultConnection connection string is not configured.");
        }

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            MultipleActiveResultSets = false
        };

        return builder.ConnectionString;
    }
}
