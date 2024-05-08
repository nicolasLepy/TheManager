using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm.persistance.nhibernate.mappings;

namespace tm.persistance.nhibernate
{
    public class NHibernateProvider : IPersistanceProvider
    {
        public Game Load()
        {
            throw new NotImplementedException();
        }

        //Fluent NHibernate
        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
              .Database(
                SQLiteConfiguration.Standard
                  .UsingFile("FirstProject.db")
              )
              //.Mappings(m => m.FluentMappings.AddFromAssemblyOf<PlayerMap>())
              .Mappings(m => m.FluentMappings.Add<PlayerMap>())
              .BuildSessionFactory();
        }

        //Fluent NHibernate
        public void Save(Game game)
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {

                    foreach (Player player in game.kernel.Players)
                    {
                        // save both stores, this saves everything else via cascading
                        session.SaveOrUpdate(player);
                    }

                    transaction.Commit();
                }
            }

        }

        //NHibernate
        /*public void Save(Game game)
        {
            foreach(Player player in game.kernel.Players)
            {
                ISession session = NHibernateHelper.OpenSession();
                try
                {
                    using (ITransaction tx = session.BeginTransaction())
                    {
                        session.Save(player);
                        tx.Commit();
                    }
                }
                finally
                {
                    NHibernateHelper.CloseSession();
                }

            }
        }*/
    }
}
