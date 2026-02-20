using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;

namespace SzerszamKolcsonzo.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static AppDbContext Create(string dbName = "")
        {
            if (string.IsNullOrEmpty(dbName))
                dbName = Guid.NewGuid().ToString(); // minden teszt saját DB-t kap

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated(); // seed adatok betöltése
            return context;
        }

        public static AppDbContext CreateEmpty(string dbName = "")
        {
            if (string.IsNullOrEmpty(dbName))
                dbName = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
            // EnsureCreated() nélkül → üres DB, seed nélkül
        }
    }
}