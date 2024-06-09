namespace VoqooePlanner.DbContexts
{
    public interface IVoqooeDbContextFactory
    {
        VoqooeDbContext CreateDbContext();
    }
}
