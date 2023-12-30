using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm
{

    public class TheManagerContext : DbContext
    {
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Person> Person { get; set; }

        private string filename;

        public TheManagerContext(string filename)
        {
            this.filename = filename;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Edition>().HasMany(e => e.Clubs).WithMany();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(String.Format("Filename=./{0}.db", filename));
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
                db.Person.AddRange(game.kernel.freePlayers);
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
