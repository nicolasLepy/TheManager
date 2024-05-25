using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Cfg;
using System.IO;
using tm.persistance.nhibernate.mappings;

namespace tm.persistance.nhibernate
{
    public static class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;
        private static Configuration _configuration;
        private static HbmMapping _mapping;

        public static void CloseSession()
        {
            SessionFactory.Close();
        }

        public static ISession OpenSession()
        {
            //Open and return the nhibernate session
            return SessionFactory.OpenSession();
        }

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    //Create the session factory
                    _sessionFactory = Configuration.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }

        public static Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    //Create the nhibernate configuration
                    _configuration = CreateConfiguration();
                }
                return _configuration;
            }
        }

        public static HbmMapping Mapping
        {
            get
            {
                if (_mapping == null)
                {
                    //Create the mapping
                    _mapping = CreateMapping();
                }
                return _mapping;
            }
        }

        private static Configuration CreateConfiguration()
        {
            var configuration = new Configuration();

            //Loads properties from hibernate.cfg.xml
            configuration.Configure();
            //configuration.Configure(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hibernate.cfg.xml"));

            /*configuration.SetProperty(
                "hibernate.connection.datasource",
                "java:comp/env/jdbc/yourDataSource");*/
            //Loads nhibernate mappings 
            configuration.AddDeserializedMapping(Mapping, null);

            return configuration;
        }

        private static HbmMapping CreateMapping()
        {
            var mapper = new ModelMapper();
            //Add the person mapping to the model mapper
            mapper.AddMappings(new List<System.Type> { typeof(PlayerMap) });
            //Create and return a HbmMapping of the model mapping in code
            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }
}
