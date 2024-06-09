using Microsoft.EntityFrameworkCore;

namespace VoqooePlanner.DbContexts
{
    public sealed class VoqooeDbContextFactory(string connectionString) : IVoqooeDbContextFactory
    {
        private readonly string _connectionString = connectionString;

        public VoqooeDbContext CreateDbContext()
        {
            DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(_connectionString).Options;

            return new VoqooeDbContext(options);
        }
    }
}
