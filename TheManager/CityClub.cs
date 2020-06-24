using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{

    [DataContract]
    public struct ContractOffer : IEquatable<ContractOffer>
    {
        public int Wage { get; set; }
        public int ContractDuration { get; set; }
        public Joueur Player { get; set; }

        public ContractOffer(Joueur player, int wage, int contractDuration)
        {
            Player = player;
            Wage = wage;
            ContractDuration = contractDuration;
        }

        public bool Equals(ContractOffer other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract(IsReference =true)]
    public class ClubTransfersManagement
    {
        [DataMember]
        private List<Joueur> _targetedPlayers;
        [DataMember]
        private List<ContractOffer> _offers;

        public List<Joueur> targetedPlayers { get => _targetedPlayers; }
        public List<ContractOffer> offers { get => _offers; }

        public ClubTransfersManagement()
        {
            _targetedPlayers = new List<Joueur>();
            _offers = new List<ContractOffer>();
        }
    }


    [DataContract(IsReference =true)]
    public class CityClub : Club
    {

        [DataMember]
        private int _budget;
        [DataMember]
        private Ville _city;
        [DataMember]
        private float _sponsor;
        [DataMember]
        private List<Contrat> _players;
        [DataMember]
        private HistoriqueClub _historic;
        [DataMember]
        private ClubTransfersManagement _clubTransfersManagement;
        [DataMember]
        private List<ReserveClub> _reserves;
        [DataMember]
        private bool _isFannion;

        public int budget { get => _budget; }
        public Ville city { get => _city; }
        public float sponsor { get => _sponsor; }
        public List<Contrat> contracts { get => _players; }
        public HistoriqueClub history { get => _historic; }
        public ClubTransfersManagement clubTransfersManagement { get => _clubTransfersManagement; }
        public List<ReserveClub> reserves { get => _reserves; }
        private bool isFannionTeam { get => _isFannion; }

        public float SalaryMass
        {
            get
            {
                float res = 0;

                foreach (Contrat c in _players) res += c.Salaire;

                return res;
            }
        }

        public CityClub(string name, Entraineur manager, string shortName, int reputation, int budget, int supporters, int formationCenter, Ville city, string logo, Stade stadium, string goalMusic, bool isFannion) : base(name,manager,shortName,reputation,supporters,formationCenter,logo,stadium,goalMusic)
        {
            _budget = budget;
            _city = city;
            _sponsor = 0;
            _players = new List<Contrat>();
            _historic = new HistoriqueClub();
            _clubTransfersManagement = new ClubTransfersManagement();
            _isFannion = isFannion;
            _reserves = new List<ReserveClub>();
        }

        public void AddPlayer(Contrat c)
        {
            _players.Add(c);
        }

        public void RemovePlayer(Joueur j)
        {
            Contrat toRemove = null;

            foreach (Contrat ct in _players) if (ct.Joueur == j) toRemove = ct;

            if (toRemove != null)
                _players.Remove(toRemove);
        }

        public override List<Joueur> Players()
        {
            List<Joueur> players = new List<Joueur>();
            foreach (Contrat c in _players)
                players.Add(c.Joueur);
            return players;
        }

        public override float Level()
        {
            float level = 0;

            
            List<Joueur> players = new List<Joueur>();
            foreach (Contrat c in _players) players.Add(c.Joueur);
            players.Sort(new Joueur_Niveau_Comparator());

            int total = 0;
            for(int i = 0;i<16;i++)
            {
                if (players.Count > i)
                {
                    level += players[i].Niveau;
                    total++;
                }
            }
            return level / (total+0.0f);
        }

        public void GeneratePlayer(Position p, int minAge, int maxAge, int potentialOffset = 0)
        {
            string firstName = _city.Pays().Langue.ObtenirPrenom();
            string lastName = _city.Pays().Langue.ObtenirNom();
            int birthYear = Session.Instance.Random(Session.Instance.Partie.Date.Year - maxAge, Session.Instance.Partie.Date.Year - minAge+1);

            //Method Level -> Potential
            int level = Session.Instance.Random(formationFacilities - 18, formationFacilities + 18) + potentialOffset;
            if (level < 1) level = 1;
            if (level > 99) level = 99;

            int age = Session.Instance.Partie.Date.Year - birthYear;
            int diff = 24 - age;
            int potential = level;
            if (diff > 0) potential += 3 * diff;

            if (potential > 99) potential = 99;

            /* Méthode Potentiel -> Niveau
            //Potentiel
            int potentiel = Session.Instance.Random(CentreFormation - 18, CentreFormation + 18) + decalagePotentiel;
            if (potentiel < 1) potentiel = 1;
            if (potentiel > 99) potentiel = 99;

            //Niveau
            int age = Session.Instance.Partie.Date.Year - anneeNaissance;
            int diff = 24 - age;
            int niveau = potentiel;
            if (diff > 0) niveau -= 3 * diff;
            
            if(niveau < 1) niveau = 1;
             */
            
            Joueur j = new Joueur(firstName, lastName, new DateTime(birthYear, Session.Instance.Random(1,13), Session.Instance.Random(1,29)), level, potential, this.city.Pays(), p);
            int year = Session.Instance.Random(Session.Instance.Partie.Date.Year + 1, Session.Instance.Partie.Date.Year + 5);
            contracts.Add(new Contrat(j, j.EstimerSalaire(), new DateTime(year, 7, 1), new DateTime(Session.Instance.Partie.Date.Year, Session.Instance.Partie.Date.Month, Session.Instance.Partie.Date.Day)));
        }

        public void GeneratePlayer(int minAge, int maxAge)
        {
            Position p = Position.Goalkeeper;
            int random = Session.Instance.Random(1, 13);
            if(random >= 2 && random <= 5) p = Position.Defender;
            if (random >= 6 && random <= 9) p = Position.Midfielder;
            if (random >= 10) p = Position.Striker;
            GeneratePlayer(p,minAge,maxAge);
        }

        public void ModifyBudget(int somme)
        {
            _budget += somme;
        }
        
        public void PayWages()
        {
            foreach(Contrat ct in contracts)
            {
                ModifyBudget(-ct.Salaire);
            }
        }

        public void SponsorGrant()
        {
            ModifyBudget((int)(sponsor / 12));
        }

        public void GetSponsor()
        {
            int sponsor = 0;
            float level = Level();
            if (level < 1000) sponsor = Session.Instance.Random(5000, 14000);
            else if (level < 3000) sponsor = Session.Instance.Random(85000, 323000);
            else if (level < 4000) sponsor = Session.Instance.Random(200000, 500000);
            else if (level < 5000) sponsor = Session.Instance.Random(500000, 800000);
            else if (level < 6000) sponsor = Session.Instance.Random(800000, 2500000);
            else if (level < 7000) sponsor = Session.Instance.Random(2500000, 6500000);
            else if (level < 8000) sponsor = Session.Instance.Random(6000000, 14000000);
            else if (level < 9000) sponsor = Session.Instance.Random(14000000, 23000000);
            else sponsor = Session.Instance.Random(23000000, 40000000);
            _sponsor = sponsor;
        }

        /// <summary>
        /// End of the year : update of formation facilities by the club
        /// Randomly lower the level
        /// If the club have money, it can put money in formation facilities to make it bigger
        /// </summary>
        public void UpdateFormationFacilities()
        {
            _formationFacilities -= Session.Instance.Random(1, 3);
            if (_formationFacilities < 1)
                _formationFacilities = 1;
            for (int i = 0; i<5; i++)
            {
                int price = (int)(967.50471* Math.Pow(1.12867,formationFacilities));
                if (_budget/3 > price && formationFacilities<99)
                {
                    ModifyBudget(-price);
                    _formationFacilities++;
                }
            }
        }

        /// <summary>
        /// Generate some junior at the beginning of every year for the club
        /// </summary>
        public void GenerateJuniors()
        {

            int nb = Session.Instance.Random(1, 3);

            int nbPlayersClub = contracts.Count;
            if(nbPlayersClub < 13)
            {
                nb = 13 - nbPlayersClub;
            }
            for(int i = 0; i<nb; i++)
            {
                GeneratePlayer(17,19);
            }
        }

        /// <summary>
        /// Try to prolong a contract
        /// </summary>
        /// <param name="ct">The contract to prolong</param>
        /// <returns>True if the contract was prolonged, false if not</returns>
        public bool Prolong(Contrat ct)
        {
            bool res = false;
            int wage = ct.Joueur.EstimerSalaire();
            if (wage < ct.Salaire) wage = ct.Salaire;
            wage = (int)(wage * (Session.Instance.Random(10, 14) / (10.0f)));

            bool validAge = true;
            if (ct.Joueur.Age > 32)
                if (Session.Instance.Random(1, 3) == 1) validAge = false;

            bool enoughGood = true;
            if (ct.Joueur.Age < 25 && ct.Joueur.Potentiel < Level() - 12)
                enoughGood = false;
            else if (ct.Joueur.Age >= 25 && ct.Joueur.Niveau < Level() - 12)
                enoughGood = false;

            if(_budget > 12*wage && validAge && enoughGood)
            {
                res = true;
                int year = Session.Instance.Random(Session.Instance.Partie.Date.Year + 1, Session.Instance.Partie.Date.Year + 5);
                ct.MettreAJour(wage, new DateTime(year, 7, 1));
            }

            return res;
        }

        public void UpdateTransfertList()
        {
            float clubLevel = Level();
            foreach(Contrat ct in _players)
            {
                //If a player is too bad for the club
                if (ct.Joueur.Potentiel / clubLevel < 0.80) ct.Transferable = true;
                else ct.Transferable = false;
            }
        }

        public void ReceiveOffer(Contrat contract, CityClub interestedClub, int amount, int wage, int contractDuration)
        {
            if(contract.Transferable)
            {
                if(amount > contract.Joueur.EstimerValeurTransfert())
                {
                    interestedClub.clubTransfersManagement.offers.Add(new ContractOffer(contract.Joueur, wage, contractDuration));
                    //contrat.Joueur.Offres.Add(new OffreContrat(interessee, salaire, dureeContrat));
                }
            }
            else
            {
                if(amount > contract.Joueur.EstimerValeurTransfert()*1.2f)
                {
                    interestedClub.clubTransfersManagement.offers.Add(new ContractOffer(contract.Joueur, wage, contractDuration));
                    //contrat.Joueur.Offres.Add(new OffreContrat(interessee, salaire, dureeContrat));
                }
            }
        }

        /// <summary>
        /// Génère le calendrier des matchs amicaux du club avant le début de la compétition
        /// </summary>
        public void GenerateFriendlyGamesCalendar()
        {
            Tournament championship = Championship;
            List<Club> possibleOpponents = new List<Club>();
            
            if(championship != null && championship.rounds[0] as TourChampionnat != null)
            {
                foreach (Club c in Session.Instance.Partie.Gestionnaire.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if (cv != null && Utils.Distance(cv.city, city) < 300)
                    {
                        possibleOpponents.Add(cv);
                    }

                }
                int nbGames = Session.Instance.Random(3, 6);
                if (nbGames > possibleOpponents.Count) nbGames = possibleOpponents.Count;
                for(int i = 0; i < nbGames; i++)
                {
                    Club adv = possibleOpponents[Session.Instance.Random(0, possibleOpponents.Count)];
                    possibleOpponents.Remove(adv);
                    DateTime begin = new DateTime(championship.rounds[0].Matchs[0].Jour.Year, championship.rounds[0].Matchs[0].Jour.Month, championship.rounds[0].Matchs[0].Jour.Day);
                    begin = begin.AddDays(Session.Instance.Random(-30, -10));
                    begin = begin.AddHours(Session.Instance.Random(14, 22));
                    Match game = new Match(this, adv, begin, false);
                    game.Reprogrammer(0);
                    Session.Instance.Partie.Gestionnaire.AjouterMatchAmical(game );
                }
            }
        }

        public void ConsiderateOffers()
        {
            foreach(ContractOffer oc in clubTransfersManagement.offers)
            {
                oc.Player.ConsidererOffre(oc, this);
            }

            clubTransfersManagement.offers.Clear();
        }

        public void SendOfferToPlayers()
        {
            if(clubTransfersManagement.targetedPlayers.Count > 0)
            {
                Joueur target = clubTransfersManagement.targetedPlayers[0];
                int wage = (int)(target.EstimerSalaire() * (Session.Instance.Random(80, 120) / 100.0f));
                clubTransfersManagement.offers.Add(new ContractOffer(target, wage, Session.Instance.Random(1, 5)));
            }
        }

        public void SearchFreePlayers()
        {
            clubTransfersManagement.targetedPlayers.Clear();

            float level = Level();
            Pays countryClub = city.Pays();

            int chance = 100 - (int)level;

            int playersToResearch = 20 - contracts.Count;
            if (playersToResearch < 0) playersToResearch = 0;

            int i = 0;
            bool pursue = true;
            int playersFound = 0;
            while(pursue)
            {
                Joueur j = Session.Instance.Partie.Gestionnaire.JoueursLibres[i];
                //Likely to interest the club
                if ((Session.Instance.Random(1,chance) == 1) && j.Niveau / level > 0.90f)
                {
                    //If the player has not a pro level, it has to be on the same country
                    //(unrealistic to have many foreign player in amateur club)
                    bool can = true;
                    if (j.Niveau < 60 && j.Nationalite != countryClub) can = false;
                    if(can)
                    {
                        clubTransfersManagement.targetedPlayers.Add(j);
                        playersFound++;
                    }
                }
                i++;
                if (i == Session.Instance.Partie.Gestionnaire.JoueursLibres.Count || playersFound == playersToResearch)
                    pursue = false;
            }
            clubTransfersManagement.targetedPlayers.Sort(new Joueur_Niveau_Comparator());
        }

        /// <summary>
        /// Complete a team at the end of the transfert maker if club has not enough players to follow a season
        /// Of course, generated player are very weak
        /// </summary>
        public void CompleteSquad()
        {
            List<Joueur> players = ListPlayersByPosition(Position.Goalkeeper);
            if(players.Count < 2)
            {
                for (int i = 0; i < 2 - players.Count; i++)
                    GeneratePlayer(Position.Goalkeeper, 18, 22, -(int)(formationFacilities-(formationFacilities * 0.75f)));
            }
            players = ListPlayersByPosition(Position.Defender);
            if (players.Count < 5)
            {
                for (int i = 0; i < 5 - players.Count; i++) 
                    GeneratePlayer(Position.Defender, 18, 22, -(int)(formationFacilities - (formationFacilities * 0.75f)));
            }
            
            players = ListPlayersByPosition(Position.Midfielder);
            if (players.Count < 5)
            {
                for (int i = 0; i < 5 - players.Count; i++) 
                    GeneratePlayer(Position.Midfielder, 18, 22, -(int)(formationFacilities - (formationFacilities * 0.75f)));
            }
            players = ListPlayersByPosition(Position.Striker);
            if (players.Count < 5)
            {
                for (int i = 0; i < 5 - players.Count; i++) 
                    GeneratePlayer(Position.Striker, 18, 22, -(int)(formationFacilities - (formationFacilities * 0.75f)));
            }
        }

        /// <summary>
        /// Dispatch players between first team and reserves
        /// </summary>
        public void DispatchPlayersInReserveTeams()
        {
            /*
            if(_reserves.Count > 0)
            {

                List<Joueur>[] joueurs = new List<Joueur>[1+_reserves.Count];
                for (int i = 0; i < joueurs.Length; i++) joueurs[i] = new List<Joueur>();

                List<Contrat>[] contrats = new List<Contrat>[1 + _reserves.Count];
                for (int i = 0; i < contrats.Length; i++) contrats[i] = new List<Contrat>();

                List<Contrat> equipeComplete = new List<Contrat>(_joueurs);
                foreach (Club_Reserve cr in _reserves) foreach (Contrat ct in cr.Contrats) equipeComplete.Add(ct);

                List<Joueur> joueursComplets = new List<Joueur>(Joueurs());
                foreach (Club_Reserve cr in _reserves) foreach (Joueur j in cr.Joueurs()) joueursComplets.Add(j);

                int[] equipePremiereQuotas = new int[4] { 3, 6, 6, 4 };
                int[] equipesReservesQuotas = new int[4] { 2, 5, 5, 4 };
                Poste[] postes = new Poste[4] { Poste.GARDIEN, Poste.DEFENSEUR, Poste.MILIEU, Poste.ATTAQUANT };

                //Pour tous les postes
                for(int numposte = 0; numposte < 4; numposte++)
                {
                    Poste poste = postes[numposte];
                    int quotaEquipePremiere = equipePremiereQuotas[numposte];
                    int quotaEquipeReserve = equipesReservesQuotas[numposte];

                    List<Joueur> joueursPoste = Utils.JoueursPoste(joueursComplets, poste);
                    joueursPoste.Sort(new Joueur_Niveau_Comparator());
                    //Equipe première
                    for (int i = 0; i < quotaEquipePremiere; i++)
                        if (joueursPoste.Count > 0)
                        {
                            joueurs[0].Add(joueursPoste[0]);
                            joueursPoste.RemoveAt(0);
                        }
                    //Pour les équipes réserves : 2 gardiens
                    for (int i = 1; i < _reserves.Count + 1; i++)
                    {
                        for (int j = 0; j < quotaEquipeReserve; j++)
                            if (joueursPoste.Count > 0)
                            {
                                joueurs[i].Add(joueursPoste[0]);
                                joueursPoste.RemoveAt(0);
                            }
                    }

                    //Les joueurs qui restent
                    while(joueursPoste.Count>0)
                    {
                        joueurs[joueurs.Length - 1].Add(joueursPoste[0]);
                        joueursPoste.RemoveAt(0);
                    }

                }

                //Récupérer les contrats associés aux joueurs
                for(int i = 0; i<joueurs.Length; i++)
                {
                    foreach(Joueur j in joueurs[i])
                    {
                        Contrat ct = null;
                        foreach (Contrat c in equipeComplete) if (c.Joueur == j) ct = c;

                        contrats[i].Add(ct);
                    }
                }

                //Répartir les joueurs dans les différentes équipes
                _joueurs.Clear();
                foreach(Contrat ct in contrats[0]) _joueurs.Add(ct);
                
                for (int i = 1; i<_reserves.Count+1;i++)
                {
                    _reserves[i - 1].Contrats.Clear();
                    foreach (Contrat ct in contrats[i]) _reserves[i - 1].Contrats.Add(ct);
                }

            }
            */
        }

    }
}