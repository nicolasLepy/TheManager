using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public abstract class ViewRanking : View
    {

        protected readonly Tournament _tournament;
        protected readonly int _year;
        protected readonly int _absoluteYear;

        protected readonly Dictionary<Tournament, Club> _cupsWinners = new Dictionary<Tournament, Club>();
        protected readonly Club _championshipTitleHolder;

        public ViewRanking(Tournament tournament)
        {
            _tournament = tournament;
            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(_tournament);
            Country country = localisation as Country;

            _year = -1;
            foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                foreach (KeyValuePair<int, Tournament> kvp in t.previousEditions)
                {
                    if (kvp.Value == _tournament)
                    {
                        _year = kvp.Key;
                    }
                }
            }

            //Get title holder
            _absoluteYear = _year > -1 ? _year : Session.Instance.Game.CurrentSeason;

            if(tournament.isChampionship)
            {
                //Get cups winner to add an annotation
                foreach (Tournament cup in country.Cups())
                {
                    _cupsWinners.Add(cup, _year > -1 ? cup.previousEditions[_year].Winner() : cup.Winner());
                }
                _championshipTitleHolder = country.FirstDivisionChampionship().previousEditions.ContainsKey(_absoluteYear - 1) ? country.FirstDivisionChampionship().previousEditions[_absoluteYear - 1].Winner() : null;
            }
        }

        protected void PrintSanctions(StackPanel spHost, Round round, double sizeMultiplier)
        {
            foreach (Club c in round.clubs)
            {
                int pointsDeduction = round.GetPointsDeduction(c);
                if (pointsDeduction > 0)
                {
                    List<SanctionType> clubSanctions = new List<SanctionType>();
                    if(round.pointsDeduction.ContainsKey(c))
                    {
                        foreach (PointDeduction pd in round.pointsDeduction[c])
                        {
                            if (!clubSanctions.Contains(pd.sanctionType))
                            {
                                clubSanctions.Add(pd.sanctionType);
                            }
                        }
                    }
                    string reasons = "";
                    foreach (SanctionType st in clubSanctions)
                    {
                        reasons = String.Format("{0}, {1}", reasons, st.ToString());
                    }
                    reasons = reasons.Length > 2 ? reasons.Remove(0, 2) : reasons;
                    spHost.Children.Add(ViewUtils.CreateLabel(String.Format("{0} : {1} points ({2})", c.name, -pointsDeduction, reasons), "StyleLabel2", (int)(14 * sizeMultiplier), -1));
                }
            }
            Country ctry = Session.Instance.Game.kernel.LocalisationTournament(round.Tournament) as Country;
            if(ctry != null)
            {
                foreach(Club c in round.clubs)
                {
                    if(ctry.GetRetrogradations().Keys.ToList().Contains(c))
                    {
                        spHost.Children.Add(ViewUtils.CreateLabel(String.Format("{0} retrogradé en {1}", c.name, ctry.GetRetrogradations()[c].name), "StyleLabel2", (int)(14 * sizeMultiplier), -1));
                    }
                }
            }
        }

        public abstract override void Full(StackPanel spRanking);
    }
}
