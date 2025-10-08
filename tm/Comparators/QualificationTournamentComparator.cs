using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static tm.Tournament;

namespace tm.Comparators
{
    public class QualificationTournamentComparator : IComparer<Qualification>
    {
        public int Compare(Qualification x, Qualification y)
        {
            int res = Utils.CompareQualificationTargets(x.target, y.target);
            if(res == 0)
            {
                res = y.roundId - x.roundId;
            }
            return res;
        }
    }

    public class LeagueCupApparitionComparator : IComparer<LeagueCupApparition>
    {
        public int Compare(LeagueCupApparition x, LeagueCupApparition y)
        {
            QualificationTarget qtx = new QualificationTournament(x.tournament);
            QualificationTarget qty = new QualificationTournament(y.tournament);
            int res = Utils.CompareQualificationTargets(qtx, qty);
            if(res == 0)
            {
                res = x.teams > 0 ? -1 : 0;
            }
            return res;
        }
    }
}
