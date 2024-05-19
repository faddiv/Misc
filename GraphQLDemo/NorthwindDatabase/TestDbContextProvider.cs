using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Data.Common;

namespace NorthwindDatabase
{
    public class TestDbContextProvider : IDesignTimeDbContextFactory<TestDbContext>
    {
        public const string ConnectionString = "Server=.\\SQLEXPRESS;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True";

        public TestDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>();
            options.UseSqlServer(ConnectionString);
            //options.EnableSensitiveDataLogging();
            return new TestDbContext(options.Options);
        }

        public static TestDbContext CreateDbContext(DbConnection connection)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>();
            options.UseSqlServer(connection);
            return new TestDbContext(options.Options);
        }
    }
}
