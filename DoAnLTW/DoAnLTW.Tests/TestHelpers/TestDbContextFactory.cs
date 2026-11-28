using DoAnLTW.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DoAnLTW.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static DbContext_TuiXach Create(string? databaseName = null)
    {
        var dbName = databaseName ?? Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<DbContext_TuiXach>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new DbContext_TuiXach(options);
        context.Database.EnsureCreated();
        return context;
    }
}
