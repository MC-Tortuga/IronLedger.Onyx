using Microsoft.EntityFrameworkCore;

namespace IronLedger.Infrastructure.Persistence;

public class OnyxDbContext : DbContext
{
    public OnyxDbContext(DbContextOptions<OnyxDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.ProcessedAtUtc);
        });
    }
}
