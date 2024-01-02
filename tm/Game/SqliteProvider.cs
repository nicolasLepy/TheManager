using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm.Tournaments;

namespace tm
{

    public class TheManagerContext : DbContext
    {
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Person> Person { get; set; }
        public DbSet<Match> Games { get; set; }
        public DbSet<MatchEvent> GameEvents { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Stadium> Stadium { get; set; }
        public DbSet<AudioSource> AudioSources { get; set; }
        public DbSet<Kernel> Kernel { get; set; }
        public DbSet<Options> Options { get; set; }
        public DbSet<AdministrativeDivision> Associations { get; set; }
        public DbSet<Continent> Continents { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Journalist> Journalists { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Round> Rounds { get; set; }

        private string filename;

        public TheManagerContext(string filename)
        {
            this.filename = filename;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NationalTeam>();
            modelBuilder.Entity<CityClub>();
            modelBuilder.Entity<ReserveClub>();
            modelBuilder.Entity<Manager>();
            modelBuilder.Entity<Player>();
            modelBuilder.Entity<GroupActiveRound>();
            modelBuilder.Entity<GroupInactiveRound>();
            modelBuilder.Entity<KnockoutRound>();
            modelBuilder.Entity<ChampionshipRound>();
            //modelBuilder.Entity<Edition>().HasMany(e => e.Clubs).WithMany();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(String.Format("Filename=./{0}.db", filename));

            //If when deserializing private properties remains null/0
            //modelBuilder.Entity<Foo>().Property(x => x.Version).UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
        }
    }

    public class SqliteProvider : IPersistanceProvider
    {

        private string filename;
        public SqliteProvider(string filename)
        {
            this.filename = filename;
        }

        public void Save(Game game)
        {
            using (var db = new TheManagerContext(filename))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.Person.AddRange(game.kernel.Players);
                db.Clubs.AddRange(game.kernel.Clubs);
                db.SaveChanges();
            }
        }

        public Game Load()
        {
            Game game = new Game();
            using (var db = new TheManagerContext(filename))
            {
                game.kernel.Clubs.AddRange(db.Clubs);
            }
            return game;
        }

    }
}
