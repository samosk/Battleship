using Microsoft.EntityFrameworkCore;

namespace Battleship;

public partial class BattleshipContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public BattleshipContext()
    {
    }

    public BattleshipContext(DbContextOptions<BattleshipContext> options)
        : base(options)
    {
    }

    public BattleshipContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // new overload to accept both options and configuration (useful for DI)
    public BattleshipContext(DbContextOptions<BattleshipContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Ship> Ships { get; set; }

    public virtual DbSet<Shot> Shots { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Get connection string from IConfiguration (user secrets)
        var connectionString = _configuration?.GetConnectionString("Battleship");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("No connection string found in user secrets.");
        }

        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("game_state", new[] { "SETUP", "PLAY", "END" })
            .HasPostgresEnum("ship_orientation", new[] { "VERTICAL", "HORIZONTAL" })
            .HasPostgresEnum("ship_type", new[] { "CARRIER", "BATTLESHIP", "DESTROYER", "SUBMARINE", "PATROL_BOAT" })
            .HasPostgresEnum("shot_outcome", new[] { "MISS", "HIT", "SINK" });

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
            entity.Property(e => e.X).HasColumnName("x");
            entity.Property(e => e.Y).HasColumnName("y");

            entity.HasOne(d => d.Game).WithMany(p => p.Ships)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("ship_game_id_fkey");
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
            entity.HasKey(e => e.UserId).HasName("user_pk");

            entity.ToTable("user");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
