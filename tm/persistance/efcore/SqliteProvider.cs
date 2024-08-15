using FluentNHibernate.Testing.Values;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using tm.Tournaments;


namespace tm.persistance.sqlite
{

    public class TheManagerContext : DbContext
    {

        public static readonly Microsoft.Extensions.Logging.LoggerFactory _myLoggerFactory =
            new LoggerFactory(new[] {
                new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
            });

        public DbSet<ContractOffer> ContractOffer { get; set; }
        public DbSet<PlayerHistory> PlayerHistory { get; set; }
        public DbSet<Person> Person { get; set; }
        public DbSet<Player> Player { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Journalist> Journalists { get; set; }

        public DbSet<Kernel> Kernel { get; set; }

        public DbSet<Club> Clubs { get; set; }
        public DbSet<Match> Games { get; set; }
        public DbSet<MatchEvent> GameEvents { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Stadium> Stadium { get; set; }
        public DbSet<AudioSource> AudioSources { get; set; }
        public DbSet<Options> Options { get; set; }
        public DbSet<AdministrativeDivision> Associations { get; set; }
        public DbSet<Continent> Continents { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Round> Rounds { get; set; }


        private string filename;

        public TheManagerContext(string filename)
        {
            this.filename = filename;
            ChangeTracker.LazyLoadingEnabled = false;
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public Dictionary<Type, List<String>> ignoredProperties = new Dictionary<Type, List<string>>();

        private void RegisterIgnoredProperty(Type type, String field)
        {
            if(!ignoredProperties.ContainsKey(type))
            {
                ignoredProperties[type] = new List<String>();
            }
            ignoredProperties[type].Add(field);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<NationalTeam>();
            modelBuilder.Entity<CityClub>();
            modelBuilder.Entity<ReserveClub>();
            modelBuilder.Entity<GroupsRound>();
            modelBuilder.Entity<GroupInactiveRound>();
            modelBuilder.Entity<KnockoutRound>();

            modelBuilder.Entity<Kernel>().Ignore(e => e.Clubs);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Kernel), "Clubs");
            modelBuilder.Entity<Kernel>().Ignore(e => e.world);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Kernel), "world");
            modelBuilder.Entity<Kernel>().Ignore(e => e.medias);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Kernel), "medias");

            modelBuilder.Entity<Kernel>().HasMany(e => e.Players).WithOne();
            modelBuilder.Entity<Kernel>().Navigation(e => e.Players).AutoInclude();
            modelBuilder.Entity<Kernel>().HasMany(e => e.Clubs).WithOne();
            modelBuilder.Entity<Kernel>().Navigation(e => e.Clubs).AutoInclude();
            modelBuilder.Entity<Kernel>().HasMany(e => e.freePlayers).WithOne();
            modelBuilder.Entity<Kernel>().Navigation(e => e.freePlayers).AutoInclude();


            modelBuilder.Entity<Person>().Property("_lastName");
            modelBuilder.Entity<Person>().Property("_firstName");
            modelBuilder.Entity<Person>().Property("_birthDay");
            modelBuilder.Entity<Person>().Ignore(e => e.nationality);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Person), "nationality");

            modelBuilder.Entity<Player>().Property("_level");
            modelBuilder.Entity<Player>().Property("_potential");
            modelBuilder.Entity<Player>().Property("_position");
            modelBuilder.Entity<Player>().Property("_suspended");
            modelBuilder.Entity<Player>().Property("_energy");
            modelBuilder.Entity<Player>().Property("_foundANewClubThisSeason");
            modelBuilder.Entity<Player>().Property("_inSelection");

            modelBuilder.Entity<Journalist>().Ignore(e => e.Games);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Journalist), "Games");

            modelBuilder.Entity<Player>().Ignore(e => e.Club);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Player), "Club");


            modelBuilder.Entity<Player>().HasMany(e => e.history);//.WithOne();
            modelBuilder.Entity<Player>().Navigation(e => e.history).AutoInclude();
            modelBuilder.Entity<Player>().HasMany("_offers");//.WithOne();
            modelBuilder.Entity<Player>().Navigation("_offers").AutoInclude();


            modelBuilder.Entity<PlayerHistory>().Property<int>("ID").HasColumnType("integer").ValueGeneratedOnAdd();
            modelBuilder.Entity<PlayerHistory>(b =>
            {
                b.HasKey("ID");
            });

            modelBuilder.Entity<PlayerHistory>().Ignore(e => e.Goals);  //TODO: To remove
            RegisterIgnoredProperty(typeof(PlayerHistory), "Goals");
            modelBuilder.Entity<PlayerHistory>().Ignore(e => e.GamesPlayed);  //TODO: To remove
            RegisterIgnoredProperty(typeof(PlayerHistory), "GamesPlayed");
            modelBuilder.Entity<PlayerHistory>().Ignore(e => e.Club);  //TODO: To remove
            RegisterIgnoredProperty(typeof(PlayerHistory), "Club");


            modelBuilder.Entity<ContractOffer>().Ignore(e => e.Origin);   //TODO: To remove
            RegisterIgnoredProperty(typeof(ContractOffer), "Origin");

            modelBuilder.Entity<ContractOffer>().Property<int>("ID").HasColumnType("integer").ValueGeneratedOnAdd();
            modelBuilder.Entity<ContractOffer>(b =>
            {
                b.HasKey("ID");
            });

            modelBuilder.Entity<Journalist>().Ignore(e => e.baseCity); //TODO: To remove
            RegisterIgnoredProperty(typeof(Journalist), "baseCity");

            modelBuilder.Entity<GeographicPosition>().Property<int>("ID").HasColumnType("integer").ValueGeneratedOnAdd();

            modelBuilder.Entity<GeographicPosition>(b =>
            {
                b.HasKey("ID");
            });


            /* HERE TO MAKE EVERYTHING WORK */

            modelBuilder.Entity<Match>().Ignore(e => e.home);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Match), "home");
            modelBuilder.Entity<Match>().Ignore(e => e.away);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Match), "away");
            modelBuilder.Entity<Match>().Ignore(e => e.Tournament);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Match), "Tournament");
            modelBuilder.Entity<Continent>().Ignore(e => e.archivalAssociationRanking); // TODO: To remove
            modelBuilder.Entity<Continent>().Ignore(e => e.internationalDates);  //TODO: To remove
            RegisterIgnoredProperty(typeof(Continent), "archivalAssociationRanking");

            modelBuilder.Entity<Kernel>().Ignore(e => e.Players); // TODO: To remove
            RegisterIgnoredProperty(typeof(Kernel), "Players");
            modelBuilder.Entity<Kernel>().Property(e => e.retiredPlayersCount).HasField("_retiredPlayersCount");

            modelBuilder.Entity<Kernel>().Property<int>("ID").HasColumnType("integer").ValueGeneratedOnAdd();

            modelBuilder.Entity<Kernel>(b => {b.HasKey("ID");});
            modelBuilder.Entity<Language>().Property<int>("ID").HasColumnType("integer").ValueGeneratedOnAdd();
            modelBuilder.Entity<Language>(b => { b.HasKey("ID"); });
            modelBuilder.Entity<MatchEventCommentary>().Property<int>("ID").HasColumnType("integer").ValueGeneratedOnAdd();
            modelBuilder.Entity<MatchEventCommentary>(b => { b.HasKey("ID"); });
            modelBuilder.Entity<Options>().Property<int>("ID").HasColumnType("integer").ValueGeneratedOnAdd();
            modelBuilder.Entity<Options>(b => { b.HasKey("ID"); });

            modelBuilder.Entity<GroupsRound>().Ignore(e => e.groups); // TODO: To Remove
            RegisterIgnoredProperty(typeof(GroupsRound), "groups");
            modelBuilder.Entity<Tournament>().Ignore(e => e.nextYearQualified); // TODO: To Remove
            RegisterIgnoredProperty(typeof(Tournament), "nextYearQualified");
            modelBuilder.Entity<Tournament>().Ignore(e => e.statistics); // TODO: To Remove
            RegisterIgnoredProperty(typeof(Tournament), "statistics");


            /*modelBuilder.Entity<Club>(b =>
                {
                    b.HasKey(e => e.id);
                }
            );*/


            /*modelBuilder.Entity<NationalTeam>();
            modelBuilder.Entity<CityClub>();
            modelBuilder.Entity<ReserveClub>();
            modelBuilder.Entity<Manager>();
            modelBuilder.Entity<GroupActiveRound>();
            modelBuilder.Entity<GroupInactiveRound>();
            modelBuilder.Entity<KnockoutRound>();

            modelBuilder.Entity<Round>().HasMany(e => e.clubs);
            modelBuilder.Entity<Round>().HasMany(e => e.matches);
            modelBuilder.Entity<Kernel>().HasMany(e => e.Clubs);
            modelBuilder.Entity<Kernel>().HasMany(e => e.Players);*/
            //modelBuilder.Entity<Tournament>().HasMany(e => e.nextYearQualified);
            //modelBuilder.Entity<Edition>().HasMany(e => e.Clubs).WithMany();
            /*modelBuilder.Entity<TournamentStatistics>()
            .Property(b => b.BiggestAttack)
            .HasConversion(
            c => JsonConvert.SerializeObject(c),
            c => JsonConvert.DeserializeObject<KeyValuePair<int, Club>>(c));*/

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string fullFilename = filename.EndsWith(".db") ? string.Format("Filename={0}", filename) : string.Format("Filename=./{0}.db", filename);
            optionsBuilder.UseSqlite(fullFilename);

            //optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.InvalidIncludePathError));

            //optionsBuilder.UseLoggerFactory(_myLoggerFactory);
            //optionsBuilder.EnableSensitiveDataLogging();

            //If when deserializing private properties remains null/0
            //modelBuilder.Entity<Foo>().Property(x => x.Version).UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
        }
    }

    public class EfCoreSqLiteProvider : IPersistanceProvider
    {

        private string filename;
        public EfCoreSqLiteProvider(string filename)
        {
            this.filename = filename;
        }

        public void Save(Game game)
        {
            using (var db = new TheManagerContext(filename))
            {

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                int startId = 1000000;

                List<int> registeredId = new List<int>();

                foreach(Club club in game.kernel.Clubs)
                {
                    if(registeredId.Contains(club.id))
                    {
                        club.id = startId++;
                    }
                    registeredId.Add(club.id);
                }

                db.Kernel.Add(game.kernel);
                //db.Person.AddRange(game.kernel.Players);
                //db.Player.AddRange(game.kernel.Players);
                //db.Clubs.AddRange(game.kernel.Clubs);
                
                db.SaveChanges();
            }
        }
        public Game Load()
        {
            Game game = new Game();
            using (var db = new TheManagerContext(filename))
            {
                //game.AttachKernel(db.Kernel.Find(1));
                //game.AttachKernel(db.Kernel.Include(e => e.freePlayers).First());

                //game.AttachKernel(db.Kernel.IncludeAllRecursively(db.ignoredProperties).First()) ;
                game.AttachKernel(db.Kernel.First());

                //game.AttachKernel(db.Kernel.Include(e => e.Clubs).First());
                //game.kernel.Players.AddRange(db.Player);
                //game.kernel.Clubs.AddRange(db.Clubs);
            }
            return game;
        }

    }

    public static class EfExtensions
    {
        public static IQueryable<TEntity> IncludeAllRecursively<TEntity>(this IQueryable<TEntity> queryable, Dictionary<Type, List<String>> ignoredProperties,
            int maxDepth = int.MaxValue, bool addSeenTypesToIgnoreList = true, HashSet<Type>? ignoreTypes = null)
            where TEntity : class
        {
            Console.WriteLine("[IncludeAllRecursively]");
            var type = typeof(TEntity);
            var includes = new List<string>();
            ignoreTypes ??= new HashSet<Type>();
            //maxDepth = 1;
            GetIncludeTypes(ignoredProperties, ref includes, prefix: string.Empty, type, ref ignoreTypes, addSeenTypesToIgnoreList, maxDepth);
            foreach (var include in includes)
            {
                Console.WriteLine("[Include] " + include);
                queryable = queryable.Include(include);
            }
            
            return queryable;
        }

        private static void GetIncludeTypes(Dictionary<Type, List<String>> ignoredProperties, ref List<string> includes, string prefix, Type type, ref HashSet<Type> ignoreSubTypes,
            bool addSeenTypesToIgnoreList = true, int maxDepth = int.MaxValue)
        {

            ignoreSubTypes.Add(type);
            Console.WriteLine("[GetIncludeTypes] " + type.Name);
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                bool isNotMapped = Attribute.IsDefined(property, typeof(NotMappedAttribute)) || (ignoredProperties.ContainsKey(type) && ignoredProperties[type].Contains(property.Name));
                var getter = property.GetGetMethod();
                if (getter != null && !isNotMapped)
                {
                    Console.WriteLine("[GetIncludeTypes] " + type.Name + ", " + ignoreSubTypes.Count);
                    var isVirtual = getter.IsVirtual;
                    if (isVirtual || !isVirtual)
                    {
                        var propPath = (prefix + "." + property.Name).TrimStart('.');
                        if (maxDepth <= propPath.Count(c => c == '.')) { break; }

                        //includes.Add(propPath);

                        var subType = property.PropertyType;
                        if (ignoreSubTypes.Contains(subType))
                        {
                            Console.WriteLine("[Ignore] " + subType);
                            continue;
                        }
                        else if (addSeenTypesToIgnoreList)
                        {
                            // add each type that we have processed to ignore list to prevent recursions
                            ignoreSubTypes.Add(type);
                        }

                        var isEnumerableType = subType.GetInterface(nameof(IEnumerable)) != null;
                        var genericArgs = subType.GetGenericArguments();
                        if (isEnumerableType && genericArgs.Length == 1)
                        {
                            // sub property is collection, use collection type and drill down
                            var subTypeCollection = genericArgs[0];
                            if (subTypeCollection != null)
                            {
                                includes.Add(propPath);
                                GetIncludeTypes(ignoredProperties, ref includes, propPath, subTypeCollection, ref ignoreSubTypes, addSeenTypesToIgnoreList, maxDepth);
                            }
                        }
                        else
                        {
                            // sub property is no collection, drill down directly
                            GetIncludeTypes(ignoredProperties, ref includes, propPath, subType, ref ignoreSubTypes, addSeenTypesToIgnoreList, maxDepth);
                        }
                    }
                }
            }
        }
    }

}


