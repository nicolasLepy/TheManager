﻿using System;
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
        public Player Player { get; set; }

        public ContractOffer(Player player, int wage, int contractDuration)
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
        private List<Player> _targetedPlayers;
        [DataMember]
        private List<ContractOffer> _offers;

        public List<Player> targetedPlayers { get => _targetedPlayers; }
        public List<ContractOffer> offers { get => _offers; }

        public ClubTransfersManagement()
        {
            _targetedPlayers = new List<Player>();
            _offers = new List<ContractOffer>();
        }
    }


    [DataContract(IsReference =true)]
    public class CityClub : Club
    {

        [DataMember]
        private int _budget;
        [DataMember]
        private City _city;
        [DataMember]
        private float _sponsor;
        [DataMember]
        private List<Contract> _players;
        [DataMember]
        private ClubHistory _historic;
        [DataMember]
        private ClubTransfersManagement _clubTransfersManagement;
        [DataMember]
        private List<ReserveClub> _reserves;
        [DataMember]
        private bool _isFannion;

        public int budget { get => _budget; }
        public City city { get => _city; }
        public float sponsor { get => _sponsor; }
        public List<Contract> contracts { get => _players; }
        public ClubHistory history { get => _historic; }
        public ClubTransfersManagement clubTransfersManagement { get => _clubTransfersManagement; }
        public List<ReserveClub> reserves { get => _reserves; }
        private bool isFannionTeam { get => _isFannion; }

        public float SalaryMass
        {
            get
            {
                float res = 0;

                foreach (Contract c in _players)
                {
                    res += c.wage;
                }

                return res;
            }
        }

        public CityClub(string name, Manager manager, string shortName, int reputation, int budget, int supporters, int formationCenter, City city, string logo, Stadium stadium, string goalMusic, bool isFannion) : base(name,manager,shortName,reputation,supporters,formationCenter,logo,stadium,goalMusic)
        {
            _budget = budget;
            _city = city;
            _sponsor = 0;
            _players = new List<Contract>();
            _historic = new ClubHistory();
            _clubTransfersManagement = new ClubTransfersManagement();
            _isFannion = isFannion;
            _reserves = new List<ReserveClub>();
        }

        public void AddPlayer(Contract c)
        {
            _players.Add(c);
        }

        public void RemovePlayer(Player j)
        {
            Contract toRemove = null;

            foreach (Contract ct in _players)
            {
                if (ct.player == j)
                {
                    toRemove = ct;
                }
            }

            if (toRemove != null)
            {
                _players.Remove(toRemove);
            }
        }

        public override List<Player> Players()
        {
            List<Player> players = new List<Player>();
            foreach (Contract c in _players)
            {
                players.Add(c.player);
            }
            return players;
        }

        public override float Level()
        {
            float level = 0;

            
            List<Player> players = new List<Player>();
            foreach (Contract c in _players) players.Add(c.player);
            players.Sort(new PlayerLevelComparator());

            int total = 0;
            for(int i = 0;i<16;i++)
            {
                if (players.Count > i)
                {
                    level += players[i].level;
                    total++;
                }
            }
            return level / (total+0.0f);
        }

        public void GeneratePlayer(Position p, int minAge, int maxAge, int potentialOffset = 0)
        {
            string firstName = _city.Country().language.GetFirstName();
            string lastName = _city.Country().language.GetLastName();
            int birthYear = Session.Instance.Random(Session.Instance.Game.date.Year - maxAge, Session.Instance.Game.date.Year - minAge+1);

            //Method Level -> Potential
            int level = Session.Instance.Random(formationFacilities - 18, formationFacilities + 18) + potentialOffset;
            if (level < 1)
            {
                level = 1;
            }

            if (level > 99)
            {
                level = 99;
            }

            int age = Session.Instance.Game.date.Year - birthYear;
            int diff = 24 - age;
            int potential = level;
            if (diff > 0)
            {
                potential += 3 * diff;
            }

            if (potential > 99)
            {
                potential = 99;
            }

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
            
            Player j = new Player(firstName, lastName, new DateTime(birthYear, Session.Instance.Random(1,13), Session.Instance.Random(1,29)), level, potential, this.city.Country(), p);
            int year = Session.Instance.Random(Session.Instance.Game.date.Year + 1, Session.Instance.Game.date.Year + 5);
            contracts.Add(new Contract(j, j.EstimateWage(), new DateTime(year, 7, 1), new DateTime(Session.Instance.Game.date.Year, Session.Instance.Game.date.Month, Session.Instance.Game.date.Day)));
        }

        public void GeneratePlayer(int minAge, int maxAge)
        {
            Position p = Position.Goalkeeper;
            int random = Session.Instance.Random(1, 13);
            if (random >= 2 && random <= 5)
            {
                p = Position.Defender;
            }
            if (random >= 6 && random <= 9)
            {
                p = Position.Midfielder;
            }
            if (random >= 10)
            {
                p = Position.Striker;
            }
            GeneratePlayer(p,minAge,maxAge);
        }

        public void ModifyBudget(int somme)
        {
            _budget += somme;
        }
        
        public void PayWages()
        {
            foreach(Contract ct in contracts)
            {
                ModifyBudget(-ct.wage);
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
            if (level < 1000)
            {
                sponsor = Session.Instance.Random(5000, 14000);
            }
            else if (level < 3000)
            {
                sponsor = Session.Instance.Random(85000, 323000);
            }
            else if (level < 4000)
            {
                sponsor = Session.Instance.Random(200000, 500000);
            }
            else if (level < 5000)
            {
                sponsor = Session.Instance.Random(500000, 800000);
            }
            else if (level < 6000)
            {
                sponsor = Session.Instance.Random(800000, 2500000);
            }
            else if (level < 7000)
            {
                sponsor = Session.Instance.Random(2500000, 6500000);
            }
            else if (level < 8000)
            {
                sponsor = Session.Instance.Random(6000000, 14000000);
            }
            else if (level < 9000)
            {
                sponsor = Session.Instance.Random(14000000, 23000000);
            }
            else
            {
                sponsor = Session.Instance.Random(23000000, 40000000);
            }
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
            {
                _formationFacilities = 1;
            }
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
        public bool Prolong(Contract ct)
        {
            bool res = false;
            int wage = ct.player.EstimateWage();
            if (wage < ct.wage)
            {
                wage = ct.wage;
            }
            wage = (int)(wage * (Session.Instance.Random(10, 14) / (10.0f)));

            bool validAge = true;
            if (ct.player.Age > 32)
            {
                if (Session.Instance.Random(1, 3) == 1)
                {
                    validAge = false;
                }
            }

            bool enoughGood = true;
            if (ct.player.Age < 25 && ct.player.potential < Level() - 12)
            {
                enoughGood = false;
            }
            else if (ct.player.Age >= 25 && ct.player.level < Level() - 12)
            {
                enoughGood = false;
            }

            if(_budget > 12*wage && validAge && enoughGood)
            {
                res = true;
                int year = Session.Instance.Random(Session.Instance.Game.date.Year + 1, Session.Instance.Game.date.Year + 5);
                ct.Update(wage, new DateTime(year, 7, 1));
            }

            return res;
        }

        public void UpdateTransfertList()
        {
            float clubLevel = Level();
            foreach(Contract ct in _players)
            {
                //If a player is too bad for the club
                if (ct.player.potential / clubLevel < 0.80)
                {
                    ct.isTransferable = true;
                }
                else
                {
                    ct.isTransferable = false;
                }
            }
        }

        public void ReceiveOffer(Contract contract, CityClub interestedClub, int amount, int wage, int contractDuration)
        {
            if(contract.isTransferable)
            {
                if(amount > contract.player.EstimateTransferValue())
                {
                    interestedClub.clubTransfersManagement.offers.Add(new ContractOffer(contract.player, wage, contractDuration));
                    //contrat.Joueur.Offres.Add(new OffreContrat(interessee, salaire, dureeContrat));
                }
            }
            else
            {
                if(amount > contract.player.EstimateTransferValue()*1.2f)
                {
                    interestedClub.clubTransfersManagement.offers.Add(new ContractOffer(contract.player, wage, contractDuration));
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
            
            if(championship != null && championship.rounds[0] as ChampionshipRound != null)
            {
                foreach (Club c in Session.Instance.Game.kernel.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if (cv != null && Utils.Distance(cv.city, city) < 300)
                    {
                        possibleOpponents.Add(cv);
                    }

                }
                int nbGames = Session.Instance.Random(3, 6);
                if (nbGames > possibleOpponents.Count)
                {
                    nbGames = possibleOpponents.Count;
                }
                for(int i = 0; i < nbGames; i++)
                {
                    Club adv = possibleOpponents[Session.Instance.Random(0, possibleOpponents.Count)];
                    possibleOpponents.Remove(adv);
                    DateTime begin = new DateTime(championship.rounds[0].matches[0].day.Year, championship.rounds[0].matches[0].day.Month, championship.rounds[0].matches[0].day.Day);
                    begin = begin.AddDays(Session.Instance.Random(-30, -10));
                    begin = begin.AddHours(Session.Instance.Random(14, 22));
                    Match game = new Match(this, adv, begin, false);
                    game.Reprogram(0);
                    Session.Instance.Game.kernel.AddFriendlyGame(game );
                }
            }
        }

        public void ConsiderateOffers()
        {
            foreach(ContractOffer oc in clubTransfersManagement.offers)
            {
                oc.Player.ConsiderOffer(oc, this);
            }

            clubTransfersManagement.offers.Clear();
        }

        public void SendOfferToPlayers()
        {
            if(clubTransfersManagement.targetedPlayers.Count > 0)
            {
                Player target = clubTransfersManagement.targetedPlayers[0];
                int wage = (int)(target.EstimateWage() * (Session.Instance.Random(80, 120) / 100.0f));
                clubTransfersManagement.offers.Add(new ContractOffer(target, wage, Session.Instance.Random(1, 5)));
            }
        }

        public void SearchFreePlayers()
        {
            clubTransfersManagement.targetedPlayers.Clear();

            float level = Level();
            Country countryClub = city.Country();

            int chance = 100 - (int)level;

            int playersToResearch = 20 - contracts.Count;
            if (playersToResearch < 0)
            {
                playersToResearch = 0;
            }

            int i = 0;
            bool pursue = true;
            int playersFound = 0;
            while(pursue)
            {
                Player j = Session.Instance.Game.kernel.freePlayers[i];
                //Likely to interest the club
                if ((Session.Instance.Random(1,chance) == 1) && j.level / level > 0.90f)
                {
                    //If the player has not a pro level, it has to be on the same country
                    //(unrealistic to have many foreign player in amateur club)
                    bool can = !(j.level < 60 && j.nationality != countryClub);
                    if(can)
                    {
                        clubTransfersManagement.targetedPlayers.Add(j);
                        playersFound++;
                    }
                }
                i++;
                if (i == Session.Instance.Game.kernel.freePlayers.Count || playersFound == playersToResearch)
                {
                    pursue = false;
                }
            }
            clubTransfersManagement.targetedPlayers.Sort(new PlayerLevelComparator());
        }

        /// <summary>
        /// Complete a team at the end of the transfert maker if club has not enough players to follow a season
        /// Of course, generated player are very weak
        /// </summary>
        public void CompleteSquad()
        {
            List<Player> players = ListPlayersByPosition(Position.Goalkeeper);
            if(players.Count < 2)
            {
                for (int i = 0; i < 2 - players.Count; i++)
                {
                    GeneratePlayer(Position.Goalkeeper, 18, 22, -(int)(formationFacilities-(formationFacilities * 0.75f)));                    
                }
            }
            players = ListPlayersByPosition(Position.Defender);
            if (players.Count < 5)
            {
                for (int i = 0; i < 5 - players.Count; i++) {
                    GeneratePlayer(Position.Defender, 18, 22, -(int)(formationFacilities - (formationFacilities * 0.75f)));                
                }
            }
            
            players = ListPlayersByPosition(Position.Midfielder);
            if (players.Count < 5)
            {
                for (int i = 0; i < 5 - players.Count; i++) {
                    GeneratePlayer(Position.Midfielder, 18, 22, -(int)(formationFacilities - (formationFacilities * 0.75f)));                
                }
            }
            players = ListPlayersByPosition(Position.Striker);
            if (players.Count < 5)
            {
                for (int i = 0; i < 5 - players.Count; i++)
                {
                    GeneratePlayer(Position.Striker, 18, 22, -(int)(formationFacilities - (formationFacilities * 0.75f)));
                }
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