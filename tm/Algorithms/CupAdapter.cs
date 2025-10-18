using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static tm.Tournament;
using System.Xml.Linq;
using tm.Comparators;
using tm.Tournaments;

namespace tm.Algorithms
{



    public class CupAdapter
    {

        /*public CupAdapter()
        {

        }

        public void Adapt(Tournament tournament)
        {

        }

        /// <summary>
        /// Update national cup qualifications due to annual league structure modifications
        /// </summary>
        public void UpdateCupQualifications()
        {
            Utils.Debug(Session.Instance.Game.date.ToShortDateString() + " [UpdateCupQualifications " + name + "] (" + Session.Instance.Game.kernel.LocalisationTournament(this) + ")");
            //Sauvegarde en mémoire les qualifications en coupe par défaut, elles pourraient être amenées à changer en cas de modification de la structure de la ligue
            if (!AlreadyStoredRecuperedTeams())
            {
                foreach (Round r in _rounds)
                {
                    r.baseRecuperedTeams.AddRange(new List<RecoverTeams>(r.recuperedTeams));
                }
            }

            // Les qualifications de chaque ligues à chaque tour sont remises par défaut
            foreach (Round r in _rounds)
            {
                r.recuperedTeams.Clear();
                r.recuperedTeams.AddRange(new List<RecoverTeams>(r.baseRecuperedTeams));
            }

            //Pour toutes les compétitions qui qualifient des équipes pour cette compétition sans pour autant faire partie du système de ligue :
            //Si les équipes sont qualifiées durant la phase régionale de la compétition, ces places de qualifications sont dispatchées aléatoirement au sein des régions.
            //Normalement, le UpdateCupQualifications() de la coupe nationale est appelé avant celle des compétitions régionales, donc les régions pourront gérer cette équipe supplémentaire juste après
            int idRoundPivot = GetFirstNationalRound();
            if (idRoundPivot > -1)
            {
                List<Tournament> childTournaments = new List<Tournament>(GetChildTournaments());
                childTournaments.Shuffle();
                int counter = 0;
                foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if (!t.IsInternational() && t != this && t.parent != this)
                    {
                        foreach (Round r in t.rounds)
                        {
                            for (int i = 0; i < r.qualifications.Count; i++)
                            {
                                if (r.qualifications[i].target.Tournament() != null && (r.qualifications[i].target.Tournament().parent == this || (r.qualifications[i].target.Tournament() == this && r.qualifications[i].roundId < idRoundPivot)))
                                {
                                    Tournament hostTournament = childTournaments[(counter++) % childTournaments.Count];
                                    Utils.Debug(string.Format("[Host Cup] {0} send winner of {1} to {2}", t.name, r.name, hostTournament.name));
                                    r.qualifications[i] = new Qualification(r.qualifications[i].ranking, r.qualifications[i].roundId, new QualificationTournament(hostTournament), r.qualifications[i].isNextYear, r.qualifications[i].qualifies);
                                }
                            }
                        }
                    }
                }
            }

            bool leagueCupLike = idRoundPivot == -1;
            foreach (Round r in _rounds)
            {
                foreach (RecoverTeams rt in r.baseRecuperedTeams)
                {
                    if (rt.Method == RecuperationMethod.QualifiedForInternationalCompetition || rt.Method == RecuperationMethod.NotQualifiedForInternationalCompetitionWorst || rt.Method == RecuperationMethod.NotQualifiedForInternationalCompetitionBest)
                    {
                        leagueCupLike = leagueCupLike && true;
                    }
                }
            }
            int[] teamsFromOutsideLeagueSystem = new int[_rounds.Count];

            //if (leagueCupLike)
            if (idRoundPivot == -1 && parent == null) //Regional cup (not regional paths of a national cup) are updated following league cup algorithm
            {
                UpdateLeagueCupQualifications();
            }
            else
            {
                List<LeagueCupApparition> leagueCupApparitions = new List<LeagueCupApparition>();
                for (int i = 0; i < _rounds.Count; i++)
                {
                    teamsFromOutsideLeagueSystem[i] = 0;
                }

                //Garde en mémoire les équipes qui participent à la compétition sans participer aux ligues (cas des équipes outre-mer en coupe de France) afin de garder leurs places.
                //On ne considère pas ici les chemins régionaux (t.parent.Value != null) car seront comptés après à l'aide des attributs Round.teamsByAssociation pour chaque tour
                foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if (!t.IsInternational() && t != this && t.parent != this)
                    {
                        foreach (Round r in t.rounds)
                        {
                            foreach (Qualification q in r.qualifications)
                            {
                                if (q.target.Tournament() == this)
                                {
                                    teamsFromOutsideLeagueSystem[q.roundId] += (q.qualifies != 0 ? q.qualifies : 1);
                                }
                            }
                        }
                    }
                }
                foreach (Round r in this.rounds)
                {
                    foreach (KeyValuePair<Association, int> regionalPath in r.teamsByAssociation)
                    {
                        teamsFromOutsideLeagueSystem[rounds.IndexOf(r)] += regionalPath.Value;
                    }
                }

                foreach (int i in teamsFromOutsideLeagueSystem) Console.WriteLine("[Update " + this.name + "] teamsFromOutsideLeagueSystem round " + i);
                //Obtient le nombre d'équipes une fois toutes les ligues entrées dans la compétition (64 en Coupe de France)
                //Dans le cas d'une phase qualificative régionale avant la phase nationale, 
                int teamsAtTheLastRound = 2;
                if (parent != null)
                {
                    Association concernedRegion = Session.Instance.Game.kernel.LocalisationTournament(this);
                    foreach (Round r in parent.rounds)
                    {
                        if (r.teamsByAssociation.ContainsKey(concernedRegion))
                        {
                            teamsAtTheLastRound = r.teamsByAssociation[concernedRegion] * 2;
                        }
                    }
                }

                int teamsKnockout = 0;
                int j = 0;
                for (int i = _rounds.Count - 1; i >= 0 && teamsKnockout == 0; i--)
                {
                    Round r = _rounds[i];
                    if (r.recuperedTeams.Count > 0)
                    {
                        teamsKnockout = teamsAtTheLastRound * (int)Math.Pow(2, j);
                    }
                    j++;
                }

                //Crée la liste qui résume à quel moment chaque division entre dans la compétition et combien d'équipes entrent par division
                foreach (Round r in _rounds)
                {
                    List<RecoverTeams> recovers = new List<RecoverTeams>(r.recuperedTeams);
                    foreach (RecoverTeams rt in recovers)
                    {
                        Round rtRound = rt.Source as Round;
                        if (rtRound != null)
                        {
                            int roundsClubCount = _parent == null ? rtRound.CountWithoutReserves() : rtRound.CountWithoutReserves(Session.Instance.Game.kernel.LocalisationTournament(this));
                            int clubsCount = roundsClubCount;
                            RecoverTeams otherRecoverTeams = GetOtherRecoverTeamsOfRound(rt);
                            if (otherRecoverTeams.Source != null)
                            {
                                clubsCount = otherRecoverTeams.Method == RecuperationMethod.Worst ? -rt.Number : Math.Max(0, clubsCount - otherRecoverTeams.Number);
                            }
                            else
                            {
                                if (rt.Method == RecuperationMethod.Best)
                                {
                                    clubsCount = -clubsCount;
                                }
                            }
                            //Du au changement de structure de ligue d'une année sur l'autre, éviter qu'on demande à une ligue plus d'équipe qu'elle n'en a
                            if (clubsCount > roundsClubCount)
                            {
                                clubsCount = roundsClubCount;
                            }
                            if (clubsCount < -roundsClubCount)
                            {
                                clubsCount = -roundsClubCount;
                            }
                            leagueCupApparitions.Add(new LeagueCupApparition(clubsCount, rounds.IndexOf(r), rtRound.Tournament));
                        }
                    }
                }

                //Trie la liste en fonction du niveau de la compétition (et si jamais une ligue entre dans la compétition en deux fois avec meilleurs/plus mauvaises équipes)
                leagueCupApparitions.Sort(new LeagueCupApparitionComparator());

                //roundStart désigne le tour où les dernières équipes entrent en compétition
                int roundStart = 0;
                foreach (LeagueCupApparition lca in leagueCupApparitions)
                {
                    Console.WriteLine("[" + name + "]" + lca.tournament.name + ", " + lca.teams + " équipes au tour " + lca.apparitionRound + "(Best ? : " + lca.isBestTeams + ")");
                    roundStart = lca.apparitionRound > roundStart ? lca.apparitionRound : roundStart;
                }

                bool noTeamsCongestion = false;

                int currentTeamsCount = 0;
                int additionalTeams = 0;

                //Remonte la compétition. Si une fois remonté au premier tour le nombre d'équipes en lice dans la compétition est négatif,
                // c'est qu'il y a trop d'équipes qui sont entrés dans la compétition en haut. On recherche donc en premier lieu des ligues qui entrent deux fois
                // dans la compétition pour ne la faire entrer qu'une fois au tour le plus bas. 
                //TODO: Ajouter une troisième condition à l'étape suivante qui prend en compte ce cas de figure ?
                while (!noTeamsCongestion)
                {
                    currentTeamsCount = teamsKnockout;// - teamsFromOutsideLeagueSystem[roundStart];
                    additionalTeams = 0;
                    //Remonte la compétition pour récupérer le nombre d'équipes à ajouter au premier tour
                    for (int i = roundStart - 1; i > -1; i--)
                    {
                        //We remove all teams that will be added to the next round
                        additionalTeams = CountAdditionnalTeamsRound(i + 1, leagueCupApparitions);
                        additionalTeams += teamsFromOutsideLeagueSystem[i + 1]; // currentTeamsCount -= teamsFromOutsideLeagueSystem[i];
                        currentTeamsCount = (currentTeamsCount - additionalTeams) * 2;
                        Console.WriteLine("[Round " + i + " (" + _rounds[i].name + ")] Will add " + additionalTeams + " teams. " + currentTeamsCount + " teams (" + currentTeamsCount / 2 + " matchs). Removed " + teamsFromOutsideLeagueSystem[i] + " teams");
                    }

                    //Trop d'équipes sont entrées en compétition, on fait remonter d'un tour les équipes de la plus basse division qui entrent le plus tardivement dans la compétition, tant qu'il n'y a pas de place pour les équipes qui entrent au premier tour
                    if (currentTeamsCount < 0)
                    {
                        Console.WriteLine("[Cas currentTeamsCount < 0]");

                        LeagueCupApparition lca = leagueCupApparitions.OrderByDescending(l => l.apparitionRound).ThenByDescending(l => Session.Instance.Game.kernel.worldAssociation.TournamentLevel(l.tournament)).FirstOrDefault();
                        if (lca != default(LeagueCupApparition) && lca.apparitionRound > 0)
                        {
                            //Met à jour la LeagueCupApparition correspondante
                            lca.apparitionRound -= 1;
                            Round r = _rounds[lca.apparitionRound + 1];
                            Round rLca = _rounds[lca.apparitionRound];
                            bool ok = false;
                            RecoverTeams swapped = default(RecoverTeams);
                            for (int i = 0; i < r.recuperedTeams.Count && !ok; i++)
                            {
                                if ((r.recuperedTeams[i].Source as Round).Tournament == lca.tournament)
                                {
                                    ok = true;
                                    swapped = r.recuperedTeams[i];
                                    rLca.recuperedTeams.Add(r.recuperedTeams[i]);
                                    r.recuperedTeams.RemoveAt(i);
                                }
                            }
                            ok = false;
                            //Met à jour les RecoverTeams après la modification
                            for (int i = 0; i < rLca.recuperedTeams.Count && !ok; i++)
                            {
                                RecoverTeams t = rLca.recuperedTeams[i];
                                if (!t.Equals(swapped) && t.Source == swapped.Source)
                                {
                                    ok = true;
                                    int teams = swapped.Number + t.Number;
                                    if (t.Method == RecuperationMethod.Worst)
                                    {
                                        rLca.recuperedTeams[rLca.recuperedTeams.Count - 1] = new RecoverTeams(swapped.Source, teams, swapped.Method);
                                        rLca.recuperedTeams.RemoveAt(i);
                                    }
                                    else if (swapped.Method == RecuperationMethod.Worst)
                                    {
                                        rLca.recuperedTeams[i] = new RecoverTeams(t.Source, teams, t.Method);
                                        rLca.recuperedTeams.RemoveAt(rLca.recuperedTeams.Count - 1);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        noTeamsCongestion = true;
                    }
                }

                additionalTeams = CountAdditionnalTeamsRound(0, leagueCupApparitions);
                Utils.Debug(additionalTeams + " >= " + currentTeamsCount + " ?");
                //Petit fix pour corriger les approximation du ratio. Une équipe est retirée à la plus basse division
                if (additionalTeams - currentTeamsCount == 1)
                {
                    Utils.Debug("[Fix applied]");
                    int minApp = -1;
                    Tournament lowestTournament = null;
                    int index = 0;
                    int minIndex = 0;
                    foreach (LeagueCupApparition lca in leagueCupApparitions)
                    {
                        bool selectedLca = minApp == -1 || (lca.apparitionRound < minApp || (lca.apparitionRound == minApp && (lowestTournament == null || lca.tournament.IsBelow(new QualificationTournament(lowestTournament)))));
                        if (selectedLca)
                        {
                            minIndex = index;
                            minApp = lca.apparitionRound;
                            lowestTournament = lca.tournament;
                        }
                        index++;
                    }
                    leagueCupApparitions[minIndex].teams--;
                    //leagueCupApparitions[leagueCupApparitions.Count - 1].teams--;
                }

                //On a suffisament d'équipes dans les divisions qui entrent au premier tour, on sélectionne alors n équipes parmis ces ligues
                if (additionalTeams >= currentTeamsCount)
                {
                    Utils.Debug("Suffisament d'équipes disponibles pour les exigences du tour");
                    //On garde le nombre d'équipes maximales des ligues sans équipes réserves : leur structure ne changera pas avec les années, on garde tout (ex. le N1 au 5ème tour avec les 18 équipes au lieu de 10 équipes calculés avec la méthode du ratio)
                    Tournament lastLevelWithoutReserves = Session.Instance.Game.kernel.LocalisationTournament(this).GetLastLeagueWithoutReserves();
                    List<LeagueCupApparition> lcaAddedByAnticipation = new List<LeagueCupApparition>();
                    foreach (LeagueCupApparition lca in leagueCupApparitions)
                    {
                        if (lca.apparitionRound == 0 && ((lastLevelWithoutReserves == null && lca.isBestTeams) || (lastLevelWithoutReserves != null && lca.tournament.IsAbove(new QualificationTournament(lastLevelWithoutReserves)))))
                        {
                            currentTeamsCount -= lca.teams;
                            additionalTeams -= lca.teams;
                            UpdateRecuperedTeams(lca.teams, lca.tournament, lca.isBestTeams, false);
                            lcaAddedByAnticipation.Add(lca);
                        }
                    }
                    foreach (LeagueCupApparition lca in lcaAddedByAnticipation)
                    {
                        leagueCupApparitions.Remove(lca);
                    }
                    //Ratio d'équipes à prendre dans chaque ligue pour obtenir le nombre d'équipes ciblé
                    double ratio = additionalTeams / (currentTeamsCount + 0.0);
                    SampleTeams(leagueCupApparitions, currentTeamsCount);
                }
                //On a pas suffisament d'équipes dans les ligues inférieures qui entrent au premier tour, on supprime ce tour pour passer directement au suivant (voir plus loin si on a toujours pas suffisament d'équipes) en simulant n qualifiés parmis les équipes des ligues inférieures
                else
                {
                    Console.WriteLine("Trop peu d'équipes disponibles pour les exigences du tour, obliger de supprimer des tours");
                    int delRoundId = 0;
                    bool ok = false;
                    while (!ok)
                    {
                        Console.WriteLine("Suppression du tour ID " + delRoundId);
                        //On supprime un tour en plus, on calcule le nombre d'équipe censée s'être qualifié à ce tour (à partir du tour précédéant) et le nombre total d'équipe à ce tour (en ajoutant celles qui arrivent à ce tour)
                        delRoundId++;
                        int expectedQualified = currentTeamsCount / 2;
                        int expectedRoundTeams = expectedQualified + CountAdditionnalTeamsRound(delRoundId, leagueCupApparitions);
                        Console.WriteLine("[Tour " + delRoundId + "] On attends " + expectedQualified + " qualifiés + " + CountAdditionnalTeamsRound(delRoundId, leagueCupApparitions) + " nouvelles équipes = " + expectedRoundTeams + " équipes à ce tour");
                        int totalDispoTeams = 0;
                        for (int i = 0; i < delRoundId + 1; i++)
                        {
                            totalDispoTeams += CountAdditionnalTeamsRound(i, leagueCupApparitions);
                        }
                        Console.WriteLine(totalDispoTeams + " équipes disponibles pour le tour " + delRoundId);
                        //Si le nombre d'équipes disponibles (c'est à dire l'intégralité des équipes des tours en lice à ce moment de la compétition) est supérieur au nombre nécessaire, alors c'est bon on débute la compétition à ce tour
                        if (totalDispoTeams > expectedRoundTeams)
                        {
                            ok = true;
                            int totalApp = 0;
                            for (int i = 0; i < delRoundId; i++)
                            {
                                totalApp += CountAdditionnalTeamsRound(i, leagueCupApparitions);
                            }
                            //Créé les qualifications des ligues dans la coupe
                            List<RecoverTeams> newRecoverTeams = new List<RecoverTeams>();

                            double ratio = expectedQualified / (totalApp + 0.0); //Ratio d'équipes à prendre dans chaque ligue
                            int currentTeamsAdded = 0;
                            for (int i = 0; i < delRoundId; i++)
                            {
                                for (int k = 0; k < leagueCupApparitions.Count; k++)
                                {
                                    int teamsToAddFromLeague = (int)Math.Round(leagueCupApparitions[k].teams * ratio); //Le nombre d'équipes à prendre dans la ligue est calculé en fonction du nombre d'équipes nécessaire au tour
                                    if (leagueCupApparitions[k].apparitionRound == i)
                                    {
                                        RecoverTeams newRt = new RecoverTeams(leagueCupApparitions[k].tournament.rounds[0], teamsToAddFromLeague, leagueCupApparitions[k].isBestTeams ? RecuperationMethod.Best : RecuperationMethod.Worst);
                                        _rounds[delRoundId].recuperedTeams.Add(newRt);
                                        currentTeamsAdded += teamsToAddFromLeague;
                                        newRecoverTeams.Add(newRt);
                                        Console.WriteLine("Tour " + i + ", " + teamsToAddFromLeague + " équipes de D" + (k + 1) + " directement qualifiés au tour " + delRoundId);
                                    }
                                }
                                _rounds[i].recuperedTeams.Clear();
                            }
                            //On oublie pas de mettre à jour les qualifications des autres ligues aux tours plus loin car d'une année à l'autre le nombre d'équipes première dans une ligue a changé
                            for (int i = 0; i < leagueCupApparitions.Count; i++)
                            {
                                RecoverTeams rt = GetRecoverTeams(leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams);
                                if (leagueCupApparitions[i].apparitionRound >= delRoundId && !newRecoverTeams.Contains(rt))
                                {
                                    UpdateRecuperedTeams(leagueCupApparitions[i].teams, leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams, false);
                                }
                            }
                            Console.WriteLine(currentTeamsAdded + " équipes ajoutées sur un total de " + expectedQualified);
                            //Même fixe que l'autre cas si le calcul du ratio n'est pas tombé juste
                            if (currentTeamsAdded != expectedQualified)
                            {
                                int margin = expectedQualified - currentTeamsAdded;
                                Console.WriteLine("Marge : " + margin + " avec " + expectedQualified + " equipes a atteindres et " + currentTeamsAdded + " equipes ajoutés");
                                for (int i = 0; i < newRecoverTeams.Count && margin != 0; i++)
                                {
                                    int currentlyAddedTeamFromLeague = 0;
                                    RecoverTeams rt = newRecoverTeams[i];
                                    LeagueCupApparition lc = null;
                                    foreach (LeagueCupApparition lca in leagueCupApparitions)
                                    {
                                        if (lca.tournament == (rt.Source as Round).Tournament && lca.isBestTeams == (rt.Method == RecuperationMethod.Best))
                                        {
                                            lc = lca;
                                        }
                                    }
                                    if (rt.Source != null)
                                    {
                                        currentlyAddedTeamFromLeague = rt.Number;
                                    }
                                    if (currentlyAddedTeamFromLeague + margin <= lc.teams)
                                    {
                                        UpdateRecuperedTeams(margin, lc.tournament, lc.isBestTeams, true);
                                        margin = 0;
                                    }
                                }
                            }
                        }
                        //Pas assez d'équipe, on passe au tour suivant en mettant à jour le nombre actuel d'équipes dans la compétition
                        currentTeamsCount = expectedRoundTeams;
                    }
                }

                if (this._rounds.Count > 8)
                {
                    PrintTournamentResumeShort();
                }
            }

            //PrintTournamentResume(teamsFromOutsideLeagueSystem);
        }

        private void PrintTournamentResumeShort()
        {
            Console.WriteLine("Résumé de la compétition " + this.name);
            foreach (Round r in _rounds)
            {
                Console.WriteLine("= " + r.name + " =");
                foreach (RecoverTeams rt in r.recuperedTeams)
                {
                    Console.WriteLine(rt.Source.ToString() + " - " + rt.Number + " - " + rt.Method);
                }
            }
        }

        private void PrintTournamentResume(int[] teamsFromOutsideLeagueSystem)
        {
            Console.WriteLine("Nouveaux tours");
            int cupTeams = 0;
            for (int i = 0; i < rounds.Count; i++)
            {
                List<RecoverTeams> recoverTeams = rounds[i].recuperedTeams;
                cupTeams += TeamsCountRound(recoverTeams);
                cupTeams += i < teamsFromOutsideLeagueSystem.Length ? teamsFromOutsideLeagueSystem[i] : 0;
                Console.WriteLine("_____ Round " + i + " _____ " + rounds[i].name);
                Console.WriteLine("Initialisé le " + rounds[i].programmation.initialisation.WeekNumber + " " + rounds[i].programmation.initialisation.MidWeekGame);
                Console.WriteLine("Se joue le " + rounds[i].programmation.gamesDays[0].WeekNumber + " " + rounds[i].programmation.initialisation.MidWeekGame);
                Console.WriteLine("Cloturé le " + rounds[i].programmation.end.WeekNumber + " " + rounds[i].programmation.end.MidWeekGame);
                foreach (RecoverTeams rt in recoverTeams)
                {
                    int totalAdmTeamsCount = rt.Source.RetrieveTeams(-1, rt.Method, rounds[i].rules.Contains(Rule.OnlyFirstTeams), Session.Instance.Game.kernel.LocalisationTournament(this)).Count;
                    Console.WriteLine("+ " + (rt.Source as Round).Tournament.name + " - " + rt.Number + "/" + totalAdmTeamsCount + " - " + rt.Method);
                }
                Console.WriteLine(cupTeams + " équipes pour " + (cupTeams / 2) + " matchs");
                cupTeams /= 2;
            }
        }

        private bool AlreadyStoredRecuperedTeams()
        {
            bool alreadyStoredRecuperedTeams = false;
            foreach (Round r in _rounds)
            {
                if (r.baseRecuperedTeams.Count > 0)
                {
                    alreadyStoredRecuperedTeams = true;
                }
            }
            return alreadyStoredRecuperedTeams;
        }

        /// <summary>
        /// Sample teams from LeagueCupApparitions.
        /// Warning : LeagueCupApparition from round 0 with isBestTeams were removed.
        /// </summary>
        /// <param name="leagueCupApparitions"></param>
        /// <param name="firstRoundTeams"></param>
        /// <returns></returns>
        private void SampleTeams(List<LeagueCupApparition> leagueCupApparitions, int expectedTeamsRound0)
        {
            //Samples teams from round 0
            List<LeagueCupApparition> lcaRound0 = new List<LeagueCupApparition>();
            List<int> idxRound0 = new List<int>();
            for (int i = 0; i < leagueCupApparitions.Count; i++)
            {
                if (leagueCupApparitions[i].apparitionRound == 0)
                {
                    lcaRound0.Add(leagueCupApparitions[i]);
                    idxRound0.Add(i);
                }
                else //Directly update the recupered teams object with the same number of teams
                {
                    UpdateRecuperedTeams(leagueCupApparitions[i].teams, leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams, false);
                }
            }
            lcaRound0 = UtilsTournaments.SampleTeams(lcaRound0, expectedTeamsRound0);

            for (int i = 0; i < lcaRound0.Count; i++)
            {
                LeagueCupApparition lca = lcaRound0[i];
                int idx = idxRound0[i];
                leagueCupApparitions[idx].teams = lca.teams;
                Console.WriteLine(String.Format("Must update {0} with {1} teams at round {2}, (is best teams ? {3})", leagueCupApparitions[idx].tournament.name, leagueCupApparitions[idx].teams, leagueCupApparitions[idx].apparitionRound, leagueCupApparitions[idx].isBestTeams));
                UpdateRecuperedTeams(leagueCupApparitions[idx].teams, leagueCupApparitions[idx].tournament, leagueCupApparitions[idx].isBestTeams, false);
            }
        }

        /// <summary>
        /// Update league cup qualifications due to clubs qualified for international competitions and professionnals teams count changes
        /// Regional competitions (with parent.key != null) and national league cup competitions (with teams appearing in function of their international qualifications) are incompatible
        /// </summary>
        public void UpdateLeagueCupQualifications()
        {
            //ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(this);
            Association association = Session.Instance.Game.kernel.LocalisationTournament(this);
            //Contains new league apparition through competition for each rounds
            List<List<RecoverTeams>> newRecoverTeams = new List<List<RecoverTeams>>();
            //Contains new extra rounds created if necessary
            List<Round> newRounds = new List<Round>();

            List<int> leagueLevelsRepresented = new List<int>();

            if (parent == null && association.isStateAssociation)
            {
                int levelsCount = association.Leagues().Count;
                int[] teamsByLevel = new int[levelsCount];
                for (int i = 0; i < levelsCount; i++)
                {
                    teamsByLevel[i] = 0;
                }
                for (int i = 0; i < _rounds.Count; i++)
                {
                    Round round = _rounds[i];
                    for (int j = round.recuperedTeams.Count - 1; j >= 0; j--)
                    {
                        RecoverTeams rt = round.recuperedTeams[j];
                        int newTeamsCount = rt.Method == RecuperationMethod.Best ? rt.Source.RetrieveTeams(-1, rt.Method, true, Session.Instance.Game.kernel.LocalisationTournament(this)).Count : 0;
                        if (newTeamsCount > 0)
                        {
                            round.recuperedTeams[j] = new RecoverTeams(rt.Source, newTeamsCount, rt.Method);
                        }
                        else
                        {
                            round.recuperedTeams.RemoveAt(j);
                        }

                        if (rt.Source as Round != null)
                        {
                            leagueLevelsRepresented.Add(association.TournamentLevel((rt.Source as Round).Tournament));
                        }
                    }
                }
            }

            //First phase : go through tournament qualifications with the new league system to check if adaptations must be made
            List<int> teamsRounds = new List<int>();
            int currentTeams = 1;
            int newTeams = 0;
            for (int i = _rounds.Count - 1; i >= 0; i--)
            {
                Round round = _rounds[i];
                currentTeams = currentTeams * 2;
                newTeams = 0;
                foreach (RecoverTeams app in round.recuperedTeams)
                {
                    int newTeamsRound = TeamsCount(app);
                    newTeams += newTeamsRound;
                }
                if (currentTeams <= newTeams && i > 0)
                {
                    foreach (RecoverTeams rt in round.recuperedTeams)
                    {
                        rounds[i - 1].recuperedTeams.Add(rt);
                    }
                    round.recuperedTeams.Clear();
                    newTeams = 0;
                }
                newRecoverTeams.Add(new List<RecoverTeams>(round.recuperedTeams));
                teamsRounds.Add(currentTeams);
                currentTeams = currentTeams - newTeams;
            }

            //Replace old RecoverTeams with new RecoverTeams with actual count of clubs (non continental or continental)

            //Case national League Cup
            if (parent == null)
            {
                foreach (List<RecoverTeams> lrt in newRecoverTeams)
                {
                    for (int i = 0; i < lrt.Count; i++)
                    {
                        if (lrt[i].Method == RecuperationMethod.NotQualifiedForInternationalCompetitionWorst || lrt[i].Method == RecuperationMethod.NotQualifiedForInternationalCompetitionBest || lrt[i].Method == RecuperationMethod.QualifiedForInternationalCompetition || lrt[i].Method == RecuperationMethod.StatusPro)
                        {
                            lrt[i] = new RecoverTeams(lrt[i].Source, TeamsCount(lrt[i]), lrt[i].Method);
                        }
                    }
                }
            }

            currentTeams = -currentTeams;
            int roundCreated = 0;
            //Trois cas
            //Case 1 : currentTeams == 0
            //Parfait
            //Case 2 : currentTeams < 0
            //Pas assez d'équipes pour compléter les premiers tours, certaines équipes sont remontées au tour suivant.
            //Ex. Coupe de la Ligue : certaines équipes de L2 entrent directement au deuxième tour
            //Case 3 : currentTeams > 0
            //Trop d'équipes pour le nombre de places aux tours suivants : création d'un nouveau tour
            Utils.Debug("[currentTeams] " + currentTeams);
            if (currentTeams == 0)
            {
                Utils.Debug("Parfait !");
            }
            else if (currentTeams < 0)
            {
                Utils.Debug("Pas assez d'équipes pour compléter les premiers tours, certaines équipes sont remontées au tour suivant");
                int teamsToMoveUp = -currentTeams;
                int roundId = newRecoverTeams.Count - 1;
                //Start with first round
                while (teamsToMoveUp > 0)
                {
                    List<RecoverTeams> bestTeams = new List<RecoverTeams>();
                    List<RecoverTeams> worstTeams = new List<RecoverTeams>();
                    int missingTeams = 0;
                    //According to the count of teams to move up, some teams are moved up (bestTeams) and the other remains on the first round (worstTeams)
                    GetBestTeams(newRecoverTeams[roundId], teamsToMoveUp, false, ref bestTeams, ref worstTeams, ref missingTeams);
                    //If additionnal teams need to be promoted to next round but can't because all teams are already moved up
                    //In this case the same operation is made again with round n°1 to round n°2...
                    teamsToMoveUp = missingTeams;
                    foreach (RecoverTeams bt in bestTeams)
                    {
                        newRecoverTeams[roundId - 1].Add(bt);
                    }
                    newRecoverTeams[roundId] = worstTeams;
                    roundId -= 1;
                    teamsToMoveUp = teamsToMoveUp / 2;
                }
            }
            else if (currentTeams > 0)
            {
                int teamsToAdd = currentTeams * 2; //New round : double teams from qualified teams for the "old first round"
                Utils.Debug("Trop d'équipes pour le nombre de places aux tours suivants : création d'un nouveau tour");
                List<GameDay> availableDatesAll = association.GetAvailableCalendarDates(Session.Instance.Game.kernel.LocalisationTournament(this).isStateAssociation, 2, leagueLevelsRepresented, true, false);
                List<GameDay> availableDates = new List<GameDay>();
                int beginningCompetition = this._seasonBeginning.WeekNumber;
                int beginningRounds = rounds.First().programmation.initialisation.WeekNumber;
                //Filter to get available dates to play the new round
                foreach (GameDay ad in availableDatesAll)
                {
                    //Les dates sont centrées sur le début de la compétition -> pas de problème en cas de passage d'une année à l'autre (semaines 52 puis semaine 02 par ex.)
                    int absoluteAd = Utils.Modulo(ad.WeekNumber - beginningCompetition, 53); // TODO: Des fois 52 ou 53 semaines !
                    int absoluteBeginFirstRound = Utils.Modulo(beginningRounds - beginningCompetition, 53); // TODO: Des fois 52 ou 53 semaines !

                    if (absoluteAd < absoluteBeginFirstRound && absoluteAd > 0)
                    {
                        availableDates.Add(ad);
                    }
                }

                while (teamsToAdd > 0)
                {
                    List<RecoverTeams> bestTeams = new List<RecoverTeams>();
                    List<RecoverTeams> worstTeams = new List<RecoverTeams>();
                    int missingTeams = 0;
                    //According to the teams numbers to add to an extra round, some teams begin tournament at the current round (bestTeams) and the other play the extra round (worstTeams)
                    GetBestTeams(newRecoverTeams.Last(), teamsToAdd, true, ref bestTeams, ref worstTeams, ref missingTeams);
                    //If new rounds can't play all teams, so the missing teams are the new qualified teams from a second extra round etc.
                    teamsToAdd = missingTeams;
                    newRecoverTeams[newRecoverTeams.Count - 1] = bestTeams;
                    newRecoverTeams.Add(worstTeams);
                    Round firstRound = this.rounds[0];
                    List<GameDay> newRoundTimes = new List<GameDay>();
                    int dateIndex = (availableDates.Count - 2) - (3 * roundCreated);
                    foreach (GameDay dt in firstRound.programmation.gamesDays)
                    {
                        newRoundTimes.Add(new GameDay(availableDates[dateIndex].WeekNumber, dt.MidWeekGame, dt.YearOffset, dt.DayOffset));
                    }
                    newRounds.Add(new KnockoutRound(Session.Instance.Game.kernel.NextIdRound(), "Tour préliminaire", this, firstRound.programmation.defaultHour, newRoundTimes, new List<TvOffset>(), firstRound.phases, new GameDay(availableDates[dateIndex].WeekNumber - 1, true, firstRound.programmation.initialisation.YearOffset, firstRound.programmation.initialisation.DayOffset), new GameDay(availableDates[dateIndex].WeekNumber + 1, firstRound.programmation.initialisation.MidWeekGame, firstRound.programmation.initialisation.YearOffset, firstRound.programmation.initialisation.DayOffset), RandomDrawingMethod.Random, false, firstRound.programmation.gamesPriority)); ;
                    newRounds[newRounds.Count - 1].recuperedTeams.AddRange(worstTeams);
                    newRounds[newRounds.Count - 1].rules.AddRange(firstRound.rules);
                    newRounds[newRounds.Count - 1].qualifications.Add(new Qualification(1, 1, new QualificationTournament(this), false, -1));
                    roundCreated++;
                    if (teamsToAdd > 0)
                    {
                        teamsToAdd *= 2;
                    }
                }
            }

            //Set extraRounds to keep trace of extra rounds (needed when reset the tournament)
            _extraRounds = roundCreated;
            foreach (Round newRound in newRounds)
            {
                rounds.Insert(0, newRound);
            }

            //Set newRecoverTeams to be the official league apparition in the tournament now
            bool nonInternationalWorstAppeared = false;
            for (int i = 0; i < rounds.Count; i++)
            {
                rounds[i].recuperedTeams.Clear();
                //TODO: Is this loop really needed ? Use case : CDL with 15 continental teams
                for (int j = 0; j < newRecoverTeams[rounds.Count - i - 1].Count; j++)
                {
                    if (newRecoverTeams[rounds.Count - i - 1][j].Method == RecuperationMethod.NotQualifiedForInternationalCompetitionWorst)
                    {
                        if (!nonInternationalWorstAppeared)
                        {
                            nonInternationalWorstAppeared = true;
                        }
                        else
                        {
                            newRecoverTeams[rounds.Count - i - 1][j] = new RecoverTeams(newRecoverTeams[rounds.Count - i - 1][j].Source, newRecoverTeams[rounds.Count - i - 1][j].Number, RecuperationMethod.NotQualifiedForInternationalCompetitionBest);
                        }
                    }
                }
                rounds[i].recuperedTeams.AddRange(newRecoverTeams[rounds.Count - i - 1]);
            }

            // Qualifications to next rounds are recalibrated in case of extra rounds (because old second round in the tournament doesn't refer to the same round now)
            for (int i = 0; i < rounds.Count; i++)
            {
                for (int j = 0; j < rounds[i].qualifications.Count; j++)
                {
                    int newRoundId = i < roundCreated ? (i + 1) : (rounds[i].qualifications[j].roundId + roundCreated);
                    rounds[i].qualifications[j] = new Qualification(rounds[i].qualifications[j].ranking, newRoundId, rounds[i].qualifications[j].target, rounds[i].qualifications[j].isNextYear, rounds[i].qualifications[j].qualifies);
                }
            }
        }*/
    }
}
