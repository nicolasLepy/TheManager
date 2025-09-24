using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tm
{
    public enum QualificationTargetType
    {
        Tournament,
        ExcludeFromLeagueSystem
    }

    public abstract class QualificationTarget
    {
        private QualificationTargetType _type;

        public QualificationTargetType Type => _type;
        public abstract Tournament Tournament();

        public abstract Tournament Tournament(Club club);

        public QualificationTarget(QualificationTargetType type)
        {
            _type = type;
        }

        public abstract int GetAssociationLevel();
        public abstract int GetTournamentLevel();
        public abstract bool ToChampionshipTournament();

        public abstract bool IsInternational();

        public abstract Tournament RegisterTeamForNextEdition(Club club, int roundIndex);

        public bool IsAbove(QualificationTarget other)
        {
            bool isAbove = false;
            int otherALevel = other.GetAssociationLevel();
            int selfALevel = GetAssociationLevel();
            int otherTLevel = other.GetTournamentLevel();
            int selfTLevel = GetTournamentLevel();
            if (selfALevel < otherALevel)
            {
                isAbove = true;
            }
            else if (selfALevel == otherALevel)
            {
                isAbove = selfTLevel < otherTLevel;
            }
            return isAbove;
        }

        public bool IsBelow(QualificationTarget other)
        {
            bool isBelow = false;
            int otherALevel = other.GetAssociationLevel();
            int selfALevel = GetAssociationLevel();
            int otherTLevel = other.GetTournamentLevel();
            int selfTLevel = GetTournamentLevel();
            if (selfALevel > otherALevel)
            {
                isBelow = true;
            }
            else if (selfALevel == otherALevel)
            {
                isBelow = selfTLevel > otherTLevel;
            }
            return isBelow;
        }

        public bool SameLevel(QualificationTarget other)
        {
            int otherALevel = other.GetAssociationLevel();
            int selfALevel = GetAssociationLevel();
            int otherTLevel = other.GetTournamentLevel();
            int selfTLevel = GetTournamentLevel();
            return otherALevel == selfALevel && otherTLevel == selfTLevel;
        }

        public override bool Equals(object obj)
        {
            QualificationTarget item = obj as QualificationTarget;
            if(item == null)
            {
                return false;
            }
            else
            {
                return Tournament() == item.Tournament() && Type == item.Type;
            }
        }

    }

    public class QualificationTournament : QualificationTarget
    {
        private Tournament _tournament;

        public QualificationTournament(Tournament t) : base(QualificationTargetType.Tournament)
        {
            if (t == null)
            {
                throw new Exception("QualificationTournament constructor can't accept a null tournament");
            }
            _tournament = t;
        }

        public override Tournament Tournament()
        {
            return _tournament;
        }

        public override Tournament Tournament(Club club)
        {
            return _tournament;
        }

        public override int GetAssociationLevel()
        {
            Association tAssociation = Session.Instance.Game.kernel.LocalisationTournament(_tournament);
            return Session.Instance.Game.kernel.worldAssociation.GetLevelOfAssociation(tAssociation, 0);
        }

        public override int GetTournamentLevel()
        {
            return _tournament.level;
        }

        public override bool ToChampionshipTournament()
        {
            return _tournament.isChampionship;
        }

        public override bool IsInternational()
        {
            return _tournament.IsInternational();
        }

        public override Tournament RegisterTeamForNextEdition(Club club, int roundIndex)
        {
            _tournament.AddClubForNextYear(club, roundIndex);
            return _tournament;
        }

        public override string ToString()
        {
            return String.Format("{{[{0}] {1}}}", Type.ToString(), _tournament.name);
        }

    }

    public class QualificationExcludeLeagueSystem : QualificationTarget
    {

        private Association _association;

        public Association association => _association;

        public QualificationExcludeLeagueSystem(Association association) : base(QualificationTargetType.ExcludeFromLeagueSystem)
        {
            _association = association;
        }

        public override Tournament Tournament(Club club)
        {
            Association clubRepresentingAssociation = club.Association().GetRepresentingAssociation(_association);
            return clubRepresentingAssociation.League(1);
        }

        public override Tournament Tournament()
        {
            return null;
        }

        public override int GetAssociationLevel()
        {
            return Session.Instance.Game.kernel.worldAssociation.GetLevelOfAssociation(_association, 0) + 1;
        }

        public override int GetTournamentLevel()
        {
            return 1;
        }

        public override bool ToChampionshipTournament()
        {
            return true;
        }

        public override bool IsInternational()
        {
            return false;
        }

        public override string ToString()
        {
            return String.Format("{{[{0}] {1}}}", Type.ToString(), association.name);
        }


        public override Tournament RegisterTeamForNextEdition(Club club, int roundIndex)
        {
            Tournament topLevelLeague = null;
            Association clubRepresentingAssociation = club.Association().GetRepresentingAssociation(_association);
            if(clubRepresentingAssociation == null)
            {
                throw new Exception(String.Format("[RegisterTeamForNextEdition] No representing association found for {0}", club.name));
            }
            else
            {
                topLevelLeague = clubRepresentingAssociation.League(1);
                if(topLevelLeague == null)
                {
                    throw new Exception(String.Format("[RegisterTeamForNextEdition] {0} association have no top level league", clubRepresentingAssociation.name));
                }
                topLevelLeague.AddClubForNextYear(club, roundIndex);
            }
            return topLevelLeague;
        }

    }

    [DataContract]
    public struct Qualification : IEquatable<Qualification>
    {
        [DataMember]
        public int ranking { get; set; }
        [DataMember]
        public int roundId { get; set; }
        [DataMember]
        public QualificationTarget target { get; set; }
        [DataMember]
        public bool isNextYear { get; set; }
        [DataMember]
        public int qualifies { get; set; }

        public Qualification(int Ranking, int RoundId, QualificationTarget target, bool nextYear, int Qualifies)
        {
            this.ranking = Ranking;
            this.roundId = RoundId;
            this.target = target;
            this.isNextYear = nextYear;
            this.qualifies = Qualifies;
        }

        public bool Equals(Qualification other)
        {
            throw new NotImplementedException();
        }
    }

}
