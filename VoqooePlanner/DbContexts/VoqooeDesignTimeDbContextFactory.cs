using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VoqooePlanner.DbContexts
{
    public sealed class VoqooeDesignTimeDbContextFactory : IDesignTimeDbContextFactory<VoqooeDbContext>
    {
        public VoqooeDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder().UseSqlite("DataSource=VoqooeSystems.db").Options;
            return new VoqooeDbContext(options);
        }
    }
}
