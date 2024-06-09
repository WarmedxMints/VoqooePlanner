using Microsoft.EntityFrameworkCore;
using VoqooePlanner.DTOs;

namespace VoqooePlanner.DbContexts
{
    public sealed class VoqooeDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<VoqooeSystemDTO> Systems { get; set; }
        public DbSet<JournalCommanderDTO> JournalCommanders { get; set; }
        public DbSet<JournalEntryDTO> JournalEntries { get; set; }
        public DbSet<SettingsDTO> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JournalEntryDTO>().HasKey(u => new
            {
                u.Filename,
                u.Offset
            });

            modelBuilder.Entity<VoqooeSystemDTO>()
                .HasMany(e => e.CommanderVisits)
                .WithMany()
                .UsingEntity("CommanderVistedSystems");
        }
    }
}