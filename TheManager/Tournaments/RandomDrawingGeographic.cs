﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager.Comparators;

namespace TheManager
{
    public class RandomDrawingGeographic : IRandomDrawing
    {

        private readonly GroupsRound _round;

        public RandomDrawingGeographic(GroupsRound tour)
        {
            _round = tour;
        }

        public void RandomDrawing()
        {
            int clubsNumberByGroup = _round.clubs.Count / _round.groupsCount;
            List<Club> clubs = new List<Club>(_round.clubs);
            if(_round.groupsLocalisation.Count > 0)
            {
                for (int i = 0; i < _round.groupsCount; i++)
                {
                    GeographicPosition position = _round.groupsLocalisation[i];
                    clubs.Sort(new ClubLocalisationComparator(position));
                    for (int j = 0; j < clubsNumberByGroup; j++)
                    {
                        _round.groups[i].Add(clubs[0]);
                        clubs.RemoveAt(0);
                    }
                }
            }
            else
            {
                List<Club>[] splitClubs = Utils.GeoClustering(clubs, _round.groupsCount);
                for(int i = 0; i<_round.groupsCount; i++)
                {
                    _round.groups[i].AddRange(splitClubs[i]);
                }
            }
        }
    }
}
