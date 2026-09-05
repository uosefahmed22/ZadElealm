using Microsoft.Data.SqlClient;
using Xunit;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.UnitTests.Infrastructure;

public class SqlServerConnectionStringTests
{
    [Fact]
    public void WithoutMultipleActiveResultSets_DisablesMarsAndPreservesOtherSettings()
    {
        const string connectionString =
            "Server=localhost;Database=ZadElealm;User Id=test-user;Password=test-password;MultipleActiveResultSets=True;Encrypt=False";

        var result = SqlServerConnectionString.WithoutMultipleActiveResultSets(connectionString);
        var builder = new SqlConnectionStringBuilder(result);

        Assert.False(builder.MultipleActiveResultSets);
        Assert.Equal("ZadElealm", builder.InitialCatalog);
        Assert.Equal("test-user", builder.UserID);
    }
}
