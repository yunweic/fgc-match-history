using Microsoft.EntityFrameworkCore;

namespace MatchHistory.Api.Data;

public sealed class MatchHistoryDbContext(DbContextOptions<MatchHistoryDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<MatchSet> Sets => Set<MatchSet>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        // Trigram matching powers the fuzzy player search ("xiaoh" finds "Xiaohai").
        model.HasPostgresExtension("pg_trgm");

        model.Entity<Player>(player =>
        {
            player.Property(p => p.Id).ValueGeneratedNever();
            player.HasIndex(p => p.GamerTag)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
        });

        model.Entity<Tournament>(tournament =>
        {
            tournament.Property(t => t.Id).ValueGeneratedNever();
            tournament.HasIndex(t => t.StartAt);
        });

        model.Entity<Event>(ev =>
        {
            ev.Property(e => e.Id).ValueGeneratedNever();
            ev.HasOne(e => e.Tournament).WithMany(t => t.Events).HasForeignKey(e => e.TournamentId);
        });

        model.Entity<MatchSet>(set =>
        {
            set.ToTable("sets", table => table.HasCheckConstraint(
                "ck_sets_player_order",
                "player_a_id IS NULL OR player_b_id IS NULL OR player_a_id < player_b_id"));

            set.Property(s => s.Id).ValueGeneratedNever();
            set.HasOne(s => s.Event).WithMany(e => e.Sets).HasForeignKey(s => s.EventId);
            set.HasOne(s => s.PlayerA).WithMany().HasForeignKey(s => s.PlayerAId).OnDelete(DeleteBehavior.Restrict);
            set.HasOne(s => s.PlayerB).WithMany().HasForeignKey(s => s.PlayerBId).OnDelete(DeleteBehavior.Restrict);

            // The head-to-head lookup: both players, newest first.
            set.HasIndex(s => new { s.PlayerAId, s.PlayerBId, s.CompletedAt })
                .IsDescending(false, false, true);

            // "All sets for this player" when they are the higher id of the pair.
            set.HasIndex(s => s.PlayerBId);
        });
    }
}
