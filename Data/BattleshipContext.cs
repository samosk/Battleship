using Battleship.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Battleship;

public partial class BattleshipContext : IdentityDbContext<User>
{
    public BattleshipContext()
    { }

    public BattleshipContext(DbContextOptions<BattleshipContext> options)
        : base(options)
    { }

    public virtual DbSet<Game> Games { get; set; }
    public virtual DbSet<Ship> Ships { get; set; }
    public virtual DbSet<Shot> Shots { get; set; }
    // Note: Users DbSet is inherited from IdentityDbContext<User>

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Configures Identity tables

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("game_pk");

            entity.ToTable("game");

            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.ActiveUserId).HasColumnName("active_user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.TurnCount).HasColumnName("turn_count");
            entity.Property(e => e.User1Id).HasColumnName("user1_id");
            entity.Property(e => e.User2Id).HasColumnName("user2_id");
            entity.Property(e => e.State)
                .HasColumnType("game_state")
                .HasColumnName("state");

            entity.HasOne(d => d.ActiveUser).WithMany(p => p.GameActiveUsers)
                .HasForeignKey(d => d.ActiveUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_active_user_id_fkey");

            entity.HasOne(d => d.User1).WithMany(p => p.GameUser1s)
                .HasForeignKey(d => d.User1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_user1_id_fkey");

            entity.HasOne(d => d.User2).WithMany(p => p.GameUser2s)
                .HasForeignKey(d => d.User2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_user2_id_fkey");
        });

        modelBuilder.Entity<Ship>(entity =>
        {
            entity.HasKey(e => e.ShipId).HasName("ship_pkey");

            entity.ToTable("ship");

            entity.Property(e => e.ShipId).HasColumnName("ship_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.X).HasColumnName("x");
            entity.Property(e => e.Y).HasColumnName("y");
            entity.Property(e => e.Orientation)
                .HasColumnType("ship_orientation")
                .HasColumnName("orientation");
            entity.Property(e => e.Type)
                .HasColumnType("ship_type")
                .HasColumnName("type");

            entity.HasOne(d => d.Game).WithMany(p => p.Ships)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("ship_game_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Ships)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("ship_user_id_fkey");
        });

        modelBuilder.Entity<Shot>(entity =>
        {
            entity.HasKey(e => e.ShotId).HasName("shot_pkey");

            entity.ToTable("shot");

            entity.Property(e => e.ShotId).HasColumnName("shot_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.ShooterUserId).HasColumnName("shooter_user_id");
            entity.Property(e => e.X).HasColumnName("x");
            entity.Property(e => e.Y).HasColumnName("y");
            entity.Property(e => e.Outcome)
                .HasColumnType("shot_outcome")
                .HasColumnName("outcome");

            entity.HasOne(d => d.Game).WithMany(p => p.Shots)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("shot_game_id_fkey");

            entity.HasOne(d => d.ShooterUser).WithMany(p => p.Shots)
                .HasForeignKey(d => d.ShooterUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("shot_shooter_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            // Identity configures the key, so we only need the table name
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}