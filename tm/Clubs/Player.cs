﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Navigation;

namespace tm
{

    [DataContract(IsReference = true)]
    public class PlayerClubStatistic
    {

        
        [DataMember]
        private int _idClub;

        [DataMember]
        public int Statistic { get; set; }

        public Club Club => Session.Instance.Game.kernel.GetClubById(_idClub);

        public PlayerClubStatistic(int idClub, int statistic)
        {
            _idClub = idClub;
            Statistic = statistic;
        }
    }

    [DataContract(IsReference = true)]
    public class PlayerHistory : IEquatable<PlayerHistory>
    {
        [DataMember]
        private List<PlayerClubStatistic> _goals;
        [DataMember]
        private List<PlayerClubStatistic> _gamesPlayed;


        [DataMember]
        public int Level { get; set; }
        [DataMember]
        public int Year { get; set; }
        /// <summary>
        /// Goals scored during the current season
        /// </summary>
        public List<PlayerClubStatistic> Goals => _goals;
        /// <summary>
        /// Played games during the current season
        /// </summary>
        public List<PlayerClubStatistic> GamesPlayed => _gamesPlayed;
        [DataMember]
        public CityClub Club { get; set; }
        public PlayerHistory(int level, int year, List<PlayerClubStatistic> goals, List<PlayerClubStatistic> playedGames, CityClub club)
        {
            Level = level;
            Year = year;
            _goals = goals;
            _gamesPlayed = playedGames;
            Club = club;
        }
        
        public PlayerHistory()
        {
            _goals = new List<PlayerClubStatistic>();
            _gamesPlayed = new List<PlayerClubStatistic>();
        }

        public bool Equals(PlayerHistory other)
        {
            return (Year > other.Year);
        }
    }



    [DataContract(IsReference =true)]
    public class Player : Person
    {
        [DataMember]
        private int _level;
        [DataMember]
        private int _potential;
        [DataMember]
        private Position _position;
        [DataMember]
        private bool _suspended;
        [DataMember]
        private int _energy;
        [DataMember]
        private List<PlayerHistory> _history;
        [DataMember]
        private List<ContractOffer> _offers;
        [DataMember]
        private bool _foundANewClubThisSeason;
        [DataMember]
        private bool _inSelection;
        [DataMember]
        private CityClub _currentClub;

        public int level { get => _level; set => _level = value; }
        public int potential { get => _potential; }
        public Position position { get => _position;}
        public bool suspended { get => _suspended; set => _suspended = value; }
        public bool inSelection { get => _inSelection; set => _inSelection = value; }

        public List<PlayerHistory> history
        {
            get
            {
                return new List<PlayerHistory>(_history);
            }
        }

        public PlayerHistory Statistics
        {
            get
            {
                return _history.Last();
            }
        }

        /*public List<PlayerClubStatistic> playedGames => _playedGames;
        public List<PlayerClubStatistic> goalsScored => _goalsScored;*/

        /// <summary>
        /// Level of the player taking into consideration his energy
        /// </summary>
        public float effectiveLevel => level * ((energy / 200.0f) + 0.5f);
        
        //TODO: Maybe better to have a isRetired attribute (performance)
        public bool IsRetired
        {
            get
            {
                return !Session.Instance.Game.kernel.freePlayers.Contains(this) && Club == null;
            }
        }

        public int energy
        {
            get => _energy;
            set
            {
                _energy = value;
                if (_energy > 100)
                {
                    _energy = 100;
                }

                if (_energy < 0)
                {
                    _energy = 0;
                }
            }
        }

        public float Stars => Utils.GetStars(_level);
        public float StarsPotential => Utils.GetStars(_potential);

        public void UpdateClub(CityClub club)
        {
            _currentClub = club;
            Statistics.Club = club;
        }

        /// <summary>
        /// Current club of the player
        /// </summary>
        public CityClub Club => _currentClub;
        /*{
            get
            {
                CityClub res = null;
                foreach(Club c in Session.Instance.Game.kernel.Clubs)
                {
                    CityClub cv = c as CityClub;
                    if(cv != null)
                    {
                        foreach(Contract ct in cv.allContracts)
                        {
                            if (ct.player == this)
                            {
                                res = cv;
                            }
                        }
                    }
                }
                return res;
            }
        }*/

        public int InternationalCaps
        {
            get
            {
                int res = 0;
                foreach(PlayerHistory he in this.history)
                {
                    foreach(PlayerClubStatistic pcs in he.GamesPlayed)
                    {
                        NationalTeam nt = pcs.Club as NationalTeam;
                        res = nt != null ? res+ pcs.Statistic : res;
                    }
                }
                return res;
            }
        }

        public int InternationalGoals
        {
            get
            {
                int res = 0;
                foreach (PlayerHistory he in this.history)
                {
                    foreach (PlayerClubStatistic pcs in he.Goals)
                    {
                        NationalTeam nt = pcs.Club as NationalTeam;
                        res = nt != null ? res + pcs.Statistic: res;
                    }
                }
                return res;
            }
        }

        public Player()
        {
            _history = new List<PlayerHistory>();
            _offers = new List<ContractOffer>();
        }

        public Player(int id, string firstName, string lastName, DateTime birthday, int level, int potential, Country nationality, Position position) : base(id, firstName,lastName,birthday,nationality)
        {
            
            _level = level;
            _potential = potential;
            _position = position;
            suspended = false;
            _energy = 100;
            _history = new List<PlayerHistory>();
            _history.Add(new PlayerHistory(_level, Session.Instance.Game.date.Year + 1, new List<PlayerClubStatistic>(), new List<PlayerClubStatistic>(), null));
            _offers = new List<ContractOffer>();
            _foundANewClubThisSeason = false;
            _inSelection = false;
        }
        

        // Properties
        //TODO : nb of yellow cards this season
        //TODO : nb of red cards this season
        //TODO : nb of goals this season
        //TODO : called in selection

        public void Recover()
        {
            if (energy < 100)
            {
                energy += Session.Instance.Random(2, 6);
            }
        }

        /// <summary>
        /// Monthly wage
        /// </summary>
        /// <returns></returns>
        public int EstimateWage()
        {
            //int wage = (int)(0.292188*Math.Pow(1.1859960,level));
            int wage = (int)(1.254 * Math.Pow(1.158, level));
            switch (_position)
            {
                case Position.Goalkeeper:
                    wage = (int)(wage *0.8f);
                    break;
                case Position.Defender:
                    wage = (int)(wage * 0.9f);
                    break;
                case Position.Striker:
                    wage = (int)(wage * 1.1f);
                    break;
                default:
                    wage *= 1;
                    break;
            }
            return wage;
        }

        public int EstimateTransferValue()
        {
            int value = (int)(1000000*(0.0001298 * Math.Pow(1.166, potential)));
            switch (_position)
            {
                case Position.Goalkeeper:
                    value = (int)(value * 0.7f);
                    break;
                case Position.Defender:
                    value = (int)(value * 0.8f);
                    break;
                case Position.Midfielder:
                    value = (int)(value * 0.9f);
                    break;
                default:
                    value *= 1;
                    break;
            }
            return value;

        }

        /// <summary>
        /// Update player level, and save current level in player historic
        /// </summary>
        public void UpdateLevel()
        {
            int age = Age;

            if(age < 24)
            {
                _level += Session.Instance.Random(1, 5);
            }

            if(age > 29)
            {
                _level -= Session.Instance.Random(1, 5);
            }
            _foundANewClubThisSeason = false;
        }

        public void UpdateStatistics()
        {
            _history.Last().Level = _level;
            _history.Add(new PlayerHistory(_level, Session.Instance.Game.date.Year + 1, new List<PlayerClubStatistic>(), new List<PlayerClubStatistic>(), Club));
        }


        /// <summary>
        /// Consider a contract offer
        /// </summary>
        /// <param name="oc">The offer to consider</param>
        /// <returns>Return the result of the negocations (Successful if was accepted by the player, NoAgreement or AlreadyTransfered if not)</returns>
        public ContractOfferResult ConsiderOffer(ContractOffer oc, CityClub sender)
        {
            ContractOfferResult res = ContractOfferResult.NoAgreementWithPlayer;
            if (!_foundANewClubThisSeason)
            {
                //If the player have a club
                if (Club != null)
                {
                    //If the new club is not too bad compared as his old club, or he was too weak for his current club
                    if (sender.Level() - Club.Level() > Session.Instance.Random(-10, -5) || _level / Club.Level() < 0.75)
                    {
                        Contract hisContract = null;
                        foreach (Contract ct in Club.allContracts)
                        {
                            if (ct.player == this)
                            {
                                hisContract = ct;
                            }
                        }
                        //If proposed wage is a little bit increasing in relation to his current wage
                        if (hisContract.wage < oc.Wage * (Session.Instance.Random(100, 120) / 100.0f))
                        {
                            Club.ModifyBudget(oc.TransferIndemnity, BudgetModificationReason.TransferIndemnity);
                            sender.ModifyBudget(-oc.TransferIndemnity, BudgetModificationReason.TransferIndemnity);
                            Club.RemovePlayer(this);
                            sender.AddPlayer(new Contract(Session.Instance.Game.kernel.NextIdContract(), this, oc.Wage, new DateTime(Session.Instance.Game.date.Year + oc.ContractDuration, 7, 1), new DateTime(Session.Instance.Game.date.Year, Session.Instance.Game.date.Month, Session.Instance.Game.date.Day)));
                            UpdateClub(sender);
                            res = ContractOfferResult.Successful;
                            _foundANewClubThisSeason = true;
                        }
                    }
                }
                //It was a free player
                else
                {
                    //If wage proposed is not too bad (at least between 0.7 et 1 of his "real wage value")
                    if ((oc.Wage + 0.0f) / EstimateWage() > (Session.Instance.Random(70, 100) / 100.0f))
                    {
                        Session.Instance.Game.kernel.freePlayers.Remove(this);
                        Contract ct = new Contract(Session.Instance.Game.kernel.NextIdContract(), this, oc.Wage, new DateTime(Session.Instance.Game.date.Year + oc.ContractDuration, 7, 1), new DateTime(Session.Instance.Game.date.Year, Session.Instance.Game.date.Month, Session.Instance.Game.date.Day));
                        sender.AddPlayer(ct);
                        UpdateClub(sender);
                        res = ContractOfferResult.Successful;
                        _foundANewClubThisSeason = true;
                    }
                }
            }
            else
            {
                res = ContractOfferResult.OtherOfferAlreadyAccepted;
            }
            return res;
        }

        public List<Match> PlayedGamesThisYear()
        {
            List<Match> res = new List<Match>();

            foreach(Match match in Session.Instance.Game.kernel.Matchs)
            {
                if(match.compo1.Contains(this) || match.compo2.Contains(this))
                {
                    res.Add(match);
                }
            }

            return res;
        }

        /*
        /// <summary>
        /// Le joueur considère toutes ses offres de contrats
        /// </summary>
        public void ConsidererOffres()
        {
            foreach(OffreContrat oc in _offres)
            {
                //Si le joueur à un club
                if(Club != null)
                {
                    if (oc.Club.Niveau() - Club.Niveau() > Session.Instance.Random(-10,-5))
                    {
                        Contrat sonContrat = null;
                        foreach (Contrat ct in Club.Contrats) if (ct.Joueur == this) sonContrat = ct;
                        //Si le salaire proposé est en légère augmentation par rapport à son budget actuel
                        if((oc.Salaire+0.0f) / sonContrat.Salaire > (Session.Instance.Random(100, 120) / 100.0f))
                        {
                            Club ancien = Club;
                            Club.RetirerJoueur(this);
                            oc.Club.AjouterJoueur(new Contrat(this, oc.Salaire, new DateTime(Session.Instance.Partie.Date.Year+oc.DureeContrat,7,1)));
                        }

                    }
                }
                else
                {
                    //Si l'offre du salaire n'est pas trop mauvaise (au moins entre 0.7 et 1 de sa "vrai valeur salariale")
                    if((oc.Salaire+0.0f) / EstimerSalaire() > (Session.Instance.Random(70,100)/100.0f))
                    {
                        oc.Club.AjouterJoueur(new Contrat(this, oc.Salaire, new DateTime(Session.Instance.Partie.Date.Year + oc.DureeContrat, 7, 1)));
                    }
                }
            }
            _offres.Clear();
        }*/

    }
}