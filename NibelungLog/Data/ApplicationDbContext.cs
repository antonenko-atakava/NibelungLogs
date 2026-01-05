using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NibelungLog.Entities;

namespace NibelungLog.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<CharacterSpec> CharacterSpecs { get; set; }
    public DbSet<RaidType> RaidTypes { get; set; }
    public DbSet<Raid> Raids { get; set; }
    public DbSet<Encounter> Encounters { get; set; }
    public DbSet<PlayerEncounter> PlayerEncounters { get; set; }
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<GuildMember> GuildMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CharacterGuid).IsUnique();
            entity.HasIndex(e => e.CharacterName);
        });

        modelBuilder.Entity<CharacterSpec>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CharacterClass, e.Spec }).IsUnique();
        });

        modelBuilder.Entity<RaidType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Map, e.Difficulty, e.InstanceType }).IsUnique();
        });

        modelBuilder.Entity<Raid>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RaidId).IsUnique();
            entity.HasIndex(e => e.StartTime);
            entity.HasOne(e => e.RaidType)
                .WithMany(rt => rt.Raids)
                .HasForeignKey(e => e.RaidTypeId);
        });

        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RaidId, e.EncounterEntry, e.StartTime });
            entity.HasOne(e => e.Raid)
                .WithMany(r => r.Encounters)
                .HasForeignKey(e => e.RaidId);
        });

        modelBuilder.Entity<PlayerEncounter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PlayerId, e.EncounterId }).IsUnique();
            entity.HasOne(e => e.Player)
                .WithMany(p => p.PlayerEncounters)
                .HasForeignKey(e => e.PlayerId);
            entity.HasOne(e => e.Encounter)
                .WithMany(enc => enc.PlayerEncounters)
                .HasForeignKey(e => e.EncounterId);
            entity.HasOne(e => e.CharacterSpec)
                .WithMany(cs => cs.PlayerEncounters)
                .HasForeignKey(e => e.CharacterSpecId);
        });

        modelBuilder.Entity<Guild>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GuildId).IsUnique();
            entity.HasIndex(e => e.GuildName);
        });

        modelBuilder.Entity<GuildMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.GuildId, e.PlayerId }).IsUnique();
            entity.HasOne(e => e.Guild)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Player)
                .WithMany(p => p.GuildMemberships)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

