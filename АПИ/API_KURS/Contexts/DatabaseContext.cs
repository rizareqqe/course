using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MovieLibrary.Models;
using Pomelo.EntityFrameworkCore.MySql.Storage;

namespace Пример_1._1.Contexts
{
    public class DatabaseContext : DbContext
    {
        private readonly string connectionString;

        public DatabaseContext()
        {
            connectionString =
                "server=127.0.0.1;" +
                "port=3306;" +
                "userid=rizareqqe;" +
                "password=rizareqqe;" +
                "database=movie_database;" +
                "sslmode=none;" +
                "AllowPublicKeyRetrieval=True;";
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration)
            : base(options)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "server=127.0.0.1;port=3306;userid=rizareqqe;password=rizareqqe;database=movie_database;sslmode=none;AllowPublicKeyRetrieval=True;";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(
                    connectionString,
                    mySqlOptions => mySqlOptions.ServerVersion(ServerVersion.AutoDetect(connectionString)));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieGenre>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });

            modelBuilder.Entity<MovieGenre>()
                .HasOne(mg => mg.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(mg => mg.MovieId);

            modelBuilder.Entity<MovieGenre>()
                .HasOne(mg => mg.Genre)
                .WithMany(g => g.MovieGenres)
                .HasForeignKey(mg => mg.GenreId);

            modelBuilder.Entity<MovieActor>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);

            modelBuilder.Entity<Movie>()
                .HasOne(m => m.CuratorUser)
                .WithMany()
                .HasForeignKey(m => m.CuratorUserId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<Director> Directors { get; set; } = null!;
        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<MovieGenre> MovieGenres { get; set; } = null!;
        public DbSet<Actor> Actors { get; set; } = null!;
        public DbSet<MovieActor> MovieActors { get; set; } = null!;
        public DbSet<UserAccount> UserAccounts { get; set; } = null!;
    }
}
