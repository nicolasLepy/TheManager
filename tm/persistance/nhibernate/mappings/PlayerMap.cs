using FluentNHibernate.Mapping;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tm.persistance.nhibernate.mappings
{
    public class PlayerMap : ClassMapping<Player>
    {
        public PlayerMap()
        {

            /*Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Age);*/

            Table("PLAYER");

            Id(p => p.Id, map =>
            {
                map.Column("PLAYER_ID");
            });

            Property(p => p.firstName, map => map.Column("FIRST_NAME"));
            Property(p => p.lastName, map => map.Column("LAST_NAME"));
            
        }
    }
}
