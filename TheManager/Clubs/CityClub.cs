using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{

    public enum ContractOfferResult
    {
        Waiting,
        Successful,
        OtherOfferAlreadyAccepted,
        NoAgreementWithPlayer,
        NoAgreementWithClub
    }

    [DataContract]
    public struct ContractOffer : IEquatable<ContractOffer>
    {
        [DataMember]
        public int Wage { get; set; }
        [DataMember]
        public int ContractDuration { get; set; }
        [DataMember]
        public Player Player { get; set; }
        [DataMember]
        public int TransferIndemnity { get; set; }
        [DataMember]
        public ContractOfferResult Result { get; set; }
        [DataMember]
        public CityClub Origin { get; set; }

        public ContractOffer(Player player, int wage, int contractDuration, int transferIndemnity, CityClub origin)
        {
            Player = player;
            Wage = wage;
            ContractDuration = contractDuration;
            TransferIndemnity = transferIndemnity;
            Result = ContractOfferResult.Waiting;
            Origin = origin;
        }

        public bool Equals(ContractOffer other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract(IsReference = true)]
    public class ClubTransfersManagement
    {
        [DataMember]
        private List<Player> _targetedPlayers;
        [DataMember]
        private List<ContractOffer> _offers;
        [DataMember]
        private List<ContractOffer> _receivedOffers;
        [DataMember]
        private List<ContractOffer> _offersHistory;

        public List<Player> targetedPlayers { get => _targetedPlayers; }
        /// <summary>
        /// List of offers send by the club
        /// </summary>
        public List<ContractOffer> offers { get => _offers; }
        public List<ContractOffer> receivedOffers => _receivedOffers;
        public List<ContractOffer> offersHistory { get => _offersHistory; }

        public ClubTransfersManagement()
        {
            _targetedPlayers = new List<Player>();
            _offers = new List<ContractOffer>();
            _offersHistory = new List<ContractOffer>();
            _receivedOffers = new List<ContractOffer>();
        }
    }

    [DataContract(IsReference =true)]
    public class CityClub : Club
    {

        [DataMember]
        private List<BudgetEntry> _budgetHistory;
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
        [DataMember]
        private bool _isForbiddenToRecruit;
        [DataMember]
        private float _baseCityAttendanceMultiplier;
        [DataMember]
        private AdministrativeDivision _administrativeDivision;

        public int budget { get => _budget; }
        public City city { get => _city; set => _city = value; }
        public float sponsor { get => _sponsor; }
        public bool isForbiddenToRecruit { get => _isForbiddenToRecruit; set => _isForbiddenToRecruit = value; }
        public float baseCityAttendanceMultiplier { get => _baseCityAttendanceMultiplier; set => _baseCityAttendanceMultiplier = value; }
        public List<Contract> contracts { get => _players; }
        public AdministrativeDivision administrativeDivision => _administrativeDivision;

        /// <summary>
        /// Get the list of contracts of the club, including contracts in reserves teams
        /// </summary>
        public List<Contract> allContracts
        {
            get
            {
                List<Contract> res = new List<Contract>(_players);
                foreach(ReserveClub rc in _reserves)
                {
                    res.AddRange(rc.Contracts);
                }
                return res;
            }
        }
        public ClubHistory history { get => _historic; }
        public ClubTransfersManagement clubTransfersManagement { get => _clubTransfersManagement; }
        public List<ReserveClub> reserves { get => _reserves; }
        private bool isFannionTeam { get => _isFannion; }

        public List<BudgetEntry> budgetHistory => _budgetHistory;

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

        public CityClub(string name, Manager manager, string shortName, int reputation, int budget, int supporters, int formationCenter, City city, string logo, Stadium stadium, string goalMusic, bool isFannion, AdministrativeDivision administrativeDivision, ClubStatus status) : base(name,manager,shortName,reputation,supporters,formationCenter,logo,stadium,goalMusic, status)
        {
            _budget = budget;
            _city = city;
            _sponsor = 0;
            _players = new List<Contract>();
            _historic = new ClubHistory();
            _clubTransfersManagement = new ClubTransfersManagement();
            _isFannion = isFannion;
            _reserves = new List<ReserveClub>();
            _budgetHistory = new List<BudgetEntry>();
            _isForbiddenToRecruit = false;
            _baseCityAttendanceMultiplier = 0;
            _administrativeDivision = administrativeDivision;
        }

        public override Country Country()
        {
            return _city.Country();
        }

        public override GeographicPosition Localisation()
        {
            return _city.Position;
        }

        public override AdministrativeDivision AdministrativeDivision()
        {
            return administrativeDivision;
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


            foreach (ReserveClub rc in _reserves)
            {
                toRemove = null;

                foreach (Contract ct in rc.Contracts)
                {
                    if (ct.player == j)
                    {
                        toRemove = ct;
                    }
                }
                if (toRemove != null)
                {
                    rc.Contracts.Remove(toRemove);
                }
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

        public void GeneratePlayer(Position p, int minAge, int maxAge)
        {
            GeneratePlayer(p, minAge, maxAge, 0);
        }
        
        public void GeneratePlayer(Position p, int minAge, int maxAge, int potentialOffset)
        {
            string firstName = _city.Country().language.GetFirstName();
            string lastName = _city.Country().language.GetLastName();
            int birthYear = Session.Instance.Random(Session.Instance.Game.date.Year - maxAge, Session.Instance.Game.date.Year - minAge+1);

            //Method Level -> Potential
            int level = Session.Instance.Random(formationFacilities - 12, formationFacilities + 12) + potentialOffset;
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
                level -= 3 * diff;
            }

            if (level < 1)
            {
                level = 1;
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

        public Contract FindContract(Player p)
        {
            Contract res = null;
            foreach(Contract c in allContracts)
            {
                if(c.player == p)
                {
                    res = c;
                }
            }
            return res;
        }

        public void GeneratePlayer(int minAge, int maxAge, int offset = 0)
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
            GeneratePlayer(p,minAge,maxAge, offset);
        }

        public void ModifyBudget(int amount, BudgetModificationReason reason)
        {
            BudgetEntry entry = new BudgetEntry(Session.Instance.Game.date, amount, reason);
            _budgetHistory.Add(entry);
            _budget += amount;
        }
        
        public void PayWages()
        {
            int wage = 0;
            foreach(Contract ct in contracts)
            {
                wage += ct.wage;
            }
            foreach(ReserveClub rc in reserves)
            {
                foreach(Contract ct in rc.Contracts)
                {
                    wage += ct.wage;
                }
            }
            if(wage > 0)
            {
                ModifyBudget(-wage, BudgetModificationReason.PayWages);
            }
        }

        public void SponsorGrant()
        {
            ModifyBudget((int)(sponsor / 12), BudgetModificationReason.SponsorGrant);
        }

        public void GetSponsor()
        {
            int sponsorValue = 0;
            float level = Level();
            if (level < 10)
            {
                sponsorValue = Session.Instance.Random(5000, 14000);
            }
            else if (level < 30)
            {
                sponsorValue = Session.Instance.Random(85000, 323000);
            }
            else if (level < 40)
            {
                sponsorValue = Session.Instance.Random(200000, 500000);
            }
            else if (level < 50)
            {
                sponsorValue = Session.Instance.Random(500000, 800000);
            }
            else if (level < 60)
            {
                sponsorValue = Session.Instance.Random(800000, 2500000);
            }
            else if (level < 70)
            {
                sponsorValue = Session.Instance.Random(2500000, 6500000);
            }
            else if (level < 80)
            {
                sponsorValue = Session.Instance.Random(6000000, 14000000);
            }
            else if (level < 90)
            {
                sponsorValue = Session.Instance.Random(14000000, 23000000);
            }
            else
            {
                sponsorValue = Session.Instance.Random(23000000, 40000000);
            }
            _sponsor = sponsorValue;
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
                    ModifyBudget(-price,BudgetModificationReason.UpdateFormationFacilities);
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
            int nbRes = 0;
            for(int i = 0; i<reserves.Count;i++)
            {
                nbRes += Session.Instance.Random(1, 3);
            }

            int nbPlayersClub = contracts.Count;
            if(nbPlayersClub < 13)
            {
                nb = 13 - nbPlayersClub;
            }
            for(int i = 0; i<nb; i++)
            {
                GeneratePlayer(17,19);
            }
            for (int i = 0; i < nbRes; i++)
            {
                GeneratePlayer(17, 19, -15);
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

            float newSalaryMassForYear = (SalaryMass - ct.wage + wage)*12;

            if(_budget > newSalaryMassForYear && validAge && enoughGood)
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

            double ratioLimit = 0.80;
            //If the financial situation of the club is complicated, they will try to sell more players
            if(budget < 0)
            {
                ratioLimit = 0.92;
            }

            foreach(Contract ct in _players)
            {
                //If a player is too bad for the club
                if ((ct.player.potential / clubLevel) < ratioLimit)
                {
                    ct.isTransferable = true;
                }
                else
                {
                    ct.isTransferable = false;
                }
            }
        }

        public bool ReceiveOffer(Contract contract, CityClub interestedClub, int amount, int wage, int contractDuration)
        {
            bool accepted = false;
            if(contract.isTransferable)
            {
                if(amount >= contract.player.EstimateTransferValue())
                {
                    interestedClub.clubTransfersManagement.receivedOffers.Add(new ContractOffer(contract.player, wage, contractDuration, amount, this));
                    accepted = true;
                }
            }
            else
            {
                if(amount > contract.player.EstimateTransferValue()*1.2f)
                {
                    interestedClub.clubTransfersManagement.receivedOffers.Add(new ContractOffer(contract.player, wage, contractDuration, amount, this));
                    accepted = true;
                }
            }
            return accepted;
        }

        /// <summary>
        /// Create friendly games before beginning of season
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
                    if (cv != null && cv.Championship != null && Utils.Distance(cv.city, city) < 300/cv.Championship.level)
                    {
                        possibleOpponents.Add(cv);
                    }
                }

                int nbGames = Session.Instance.Random(2, 5);
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
                    begin = begin.AddHours(Session.Instance.Random(12, 22));
                    Match game = new Match(this, adv, begin, false);
                    Session.Instance.Game.kernel.AddFriendlyGame(game);
                    game.Reprogram(0);
                    if (!(Utils.CompareDates(game.day, begin) && game.day.CompareTo(Session.Instance.Game.date.AddDays(1)) > 0 && adv != this))
                    {
                        Session.Instance.Game.kernel.CancelFriendlyGame(game);
                    }
                }
            }
        }

        public void ConsiderateReceivedOffers()
        {
            //To be implemented
        }

        public void ConsiderateSendOffers()
        {
            for(int i = 0; i< clubTransfersManagement.offers.Count; i++){
                ContractOffer oc = clubTransfersManagement.offers[i];
                oc.Result = oc.Player.ConsiderOffer(oc, this);
                clubTransfersManagement.offers[i] = oc;

                if (Session.Instance.Game.club.Championship.rounds[0].clubs.Contains(this))
                {
                    string res = ArticleGenerator.Instance.GenerateArticle(oc, this);
                    if (res != "")
                    {
                        Article article = new Article(res, "", new DateTime(Session.Instance.Game.date.Year, Session.Instance.Game.date.Month, Session.Instance.Game.date.Day), 2);
                        Session.Instance.Game.articles.Add(article);
                    }
                }

            }

            clubTransfersManagement.offersHistory.AddRange(clubTransfersManagement.offers);
            clubTransfersManagement.offers.Clear();
        }

        public void SendOfferToPlayers()
        {
            if(clubTransfersManagement.targetedPlayers.Count > 0)
            {
                Player target = clubTransfersManagement.targetedPlayers[0];
                int wage = (int)(target.EstimateWage() * (Session.Instance.Random(80, 120) / 100.0f));
                clubTransfersManagement.offers.Add(new ContractOffer(target, wage, Session.Instance.Random(1, 5), 0, target.Club));
                clubTransfersManagement.targetedPlayers.RemoveAt(0);
            }
        }

        public int PlayerWithAcceptableLevel(float level)
        {
            int res = 0;
            foreach(Contract c in allContracts)
            {
                if(c.player.level > level - 6)
                {
                    res++;
                }
            }

            return res;
        }

        /// <summary>
        /// Evaluate a player and determine if it is worth to try to recruit him
        /// </summary>
        /// <param name="player">The player to evaluate</param>
        private void EvaluatePlayer(Player player)
        {
            
        }

        /// <summary>
        /// Search player in transfer list
        /// </summary>
        public void SearchInTransferList()
        {
            int cumulatedTransferValue = 0;
            float level = Level();
            int playersToResearch = GetPlayersToResearch(level);
            int playersFound = 0;
            int chance = 150 - (int)level;
            List<Player> transferables = Session.Instance.Game.kernel.TransferList(Championship.level, city.Country());
            transferables.Sort(new PlayerComparator(true, PlayerAttribute.LEVEL));
            int i = 0;
            while(i < transferables.Count && playersFound < playersToResearch)
            {
                Player p = transferables[i];
                int wage = (int)(p.EstimateWage() * (Session.Instance.Random(100, 120) / 100f));
                int transferValue = p.EstimateTransferValue();
                if(p.level / level > 1 && cumulatedTransferValue + transferValue < (_budget/1.5) && Session.Instance.Random(1, chance) == 1 && wage < (_budget / 18)){
                    cumulatedTransferValue += transferValue;
                    clubTransfersManagement.targetedPlayers.Add(p);
                    playersFound++;
                    int contractDuration = Session.Instance.Random(1, 5);
                    if (p.Club.ReceiveOffer(p.Club.FindContract(p), this, transferValue, wage, contractDuration))
                    {
                        clubTransfersManagement.offers.Add(new ContractOffer(p, wage, contractDuration, transferValue, p.Club));
                    }
                }
                i++;
            }
        }

        private int GetPlayersToResearch(float level)
        {
            int playersToResearch = 20 - PlayerWithAcceptableLevel(level);
            if (playersToResearch < 0)
            {
                playersToResearch = 0;
            }
            return playersToResearch;
        }

        /// <summary>
        /// Search for free players and list them
        /// </summary>
        public void SearchFreePlayers()
        {
            clubTransfersManagement.targetedPlayers.Clear();

            float level = Level();
            Country countryClub = city.Country();

            int chance = 100 - (int)level;

            int playersToResearch = GetPlayersToResearch(level);

            int i = 0;
            bool pursue = true;
            int playersFound = 0;
            List<Player> freePlayers = Session.Instance.Game.kernel.freePlayers;
            while (pursue)
            {
                Player j = freePlayers[i];
                //Likely to interest the club
                if ((Session.Instance.Random(1,chance) == 1) && j.level / level > 0.90f && j.EstimateWage() < (_budget/18))
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
            clubTransfersManagement.targetedPlayers.Sort(new PlayerComparator(true, PlayerAttribute.LEVEL));
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

            int countPlayers = this.Players().Count;
            if(_reserves.Count > 0)
            {

                List<Player>[] joueurs = new List<Player>[1+_reserves.Count];
                for (int i = 0; i < joueurs.Length; i++)
                {
                    joueurs[i] = new List<Player>();
                }
                List<Contract>[] contrats = new List<Contract>[1 + _reserves.Count];
                for (int i = 0; i < contrats.Length; i++)
                {
                    contrats[i] = new List<Contract>();
                }

                List<Contract> equipeComplete = new List<Contract>(_players);
                foreach (ReserveClub cr in _reserves)
                {
                    foreach (Contract ct in cr.Contracts)
                    {
                        equipeComplete.Add(ct);
                    }
                }

                List<Player> joueursComplets = new List<Player>(Players());
                foreach (ReserveClub cr in _reserves)
                {
                    foreach (Player j in cr.Players())
                    {
                        joueursComplets.Add(j);
                    }
                }


                int[] equipePremiereQuotas = new[] { 3, 6, 6, 4 };
                int[] equipesReservesQuotas = new[] { 2, 5, 5, 4 };
                
                Position[] postes = new Position[] { Position.Goalkeeper, Position.Defender, Position.Midfielder, Position.Striker};

                //Pour tous les postes
                for(int numposte = 0; numposte < 4; numposte++)
                {
                    Position poste = postes[numposte];
                    int quotaEquipePremiere = equipePremiereQuotas[numposte];
                    int quotaEquipeReserve = equipesReservesQuotas[numposte];

                    List<Player> joueursPoste = Utils.PlayersByPosition(joueursComplets, poste);
                    joueursPoste.Sort(new PlayerComparator(true, PlayerAttribute.LEVEL));
                    //Equipe première
                    for (int i = 0; i < quotaEquipePremiere; i++)
                    {
                        if (joueursPoste.Count > 0)
                        {
                            joueurs[0].Add(joueursPoste[0]);
                            joueursPoste.RemoveAt(0);
                        }

                    }
                    //Pour les équipes réserves : 2 gardiens
                    for (int i = 1; i < _reserves.Count + 1; i++)
                    {
                        for (int j = 0; j < quotaEquipeReserve; j++)
                        {
                            if (joueursPoste.Count > 0)
                            {
                                joueurs[i].Add(joueursPoste[0]);
                                joueursPoste.RemoveAt(0);
                            }
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
                    foreach(Player j in joueurs[i])
                    {
                        Contract ct = null;
                        foreach (Contract c in equipeComplete)
                        {
                            if (c.player == j)
                            {
                                ct = c;
                            }
                        }

                        contrats[i].Add(ct);
                    }
                }

                //Répartir les joueurs dans les différentes équipes
                _players.Clear();
                foreach (Contract ct in contrats[0])
                {
                    _players.Add(ct);
                }

                for (int i = 1; i<_reserves.Count+1;i++)
                {
                    _reserves[i - 1].Contracts.Clear();
                    foreach (Contract ct in contrats[i])
                    {
                        _reserves[i - 1].Contracts.Add(ct);
                    }
                }

            }
        }

        public override void ChangeStatus(ClubStatus newStatus)
        {
            this._status = newStatus;
            foreach(ReserveClub rc in this.reserves)
            {
                rc.ChangeStatus(newStatus);
            }
        }
    }
}