using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Algorithms
{
    /// <summary>
    /// KMeans clustering algorithm applied to clubs for geographic clustering
    /// </summary>
    public class KMeansClustering
    {
        /// <summary>
        /// Clubs out of the geographic computation of centroids and dispatched into hats (ex. ultramarines clubs)
        /// </summary>
        private List<Club> _clubsToDispatch;
        /// <summary>
        /// All clubs
        /// </summary>
        private List<Club> _clubs;
        private int _clustersCount;

        private float[,] ComputeDistance(List<Club> clubs, List<GeographicPosition> positions)
        {
            float[,] distance = new float[clubs.Count, positions.Count];
            for (int i = 0; i < clubs.Count; i++)
            {
                for (int j = 0; j < positions.Count; j++)
                {
                    distance[i, j] = Utils.Distance(clubs[i].Localisation(), positions[j]);
                }
            }
            return distance;
        }

        private List<int> FindClosestCluster(float[,] distance)
        {
            List<int> closestClusters = new List<int>();

            //Foreach clubs
            for (int i = 0; i < distance.GetLength(0); i++)
            {
                int closestCluster = -1;
                float currentMin = float.NaN;

                //Foreach clusters
                for (int j = 0; j < distance.GetLength(1); j++)
                {
                    if (distance[i, j] < currentMin || float.IsNaN(currentMin))
                    {
                        currentMin = distance[i, j];
                        closestCluster = j;
                    }
                }
                closestClusters.Add(closestCluster);
            }

            return closestClusters;
        }

        private List<GeographicPosition> ComputeCentroids(List<Club> clubs, List<int> closestClusters, int clusterCount)
        {
            List<GeographicPosition> centroids = new List<GeographicPosition>();

            for (int i = 0; i < clusterCount; i++)
            {
                float totalLat = 0;
                float totalLon = 0;
                int total = 0;
                for (int k = 0; k < closestClusters.Count; k++)
                {
                    if (closestClusters[k] == i)
                    {
                        totalLat += clubs[k].Localisation().Latitude;
                        totalLon += clubs[k].Localisation().Longitude;
                        total++;
                    }
                }
                if(total > 0)
                {
                    totalLat /= total;
                    totalLon /= total;
                }
                centroids.Add(new GeographicPosition(totalLat, totalLon));
            }

            return centroids;
        }

        /// <summary>
        /// Split clubs in equal-size geographic clusters
        /// </summary>
        public List<Club>[] CreateClusters()
        {
            List<Club>[] res = new List<Club>[_clustersCount];
            for (int i = 0; i < _clustersCount; i++)
            {
                res[i] = new List<Club>();
            }

            int[] clustersCapacity = Utils.GetClustersCapacity(_clubs.Count, _clustersCount);
            int maxIterations = 100;
            List<GeographicPosition> centroids = new List<GeographicPosition>();
            for (int i = 0; i < _clustersCount; i++)
            {
                centroids.Add(_clubs[i].Localisation());
                centroids[i] = new GeographicPosition(centroids[i].Latitude + (Session.Instance.Random(-25, 25) / 1000.0f), centroids[i].Longitude + (Session.Instance.Random(-25, 25) / 1000.0f));
            }
            for (int i = 0; i < maxIterations; i++)
            {
                List<GeographicPosition> oldCentroids = new List<GeographicPosition>(centroids);
                float[,] distance = ComputeDistance(_clubs, oldCentroids);
                List<int> closestClusters = FindClosestCluster(distance);
                centroids = ComputeCentroids(_clubs, closestClusters, _clustersCount);
            }
            for (int i = 0; i < centroids.Count; i++)
            {
                Console.WriteLine(centroids[i].Latitude + " - " + centroids[i].Longitude);
            }

            List<int>[] clubsDistance = new List<int>[_clubs.Count];
            float[,] finalDistance = ComputeDistance(_clubs, centroids);
            for (int i = 0; i < _clubs.Count; i++)
            {
                clubsDistance[i] = new List<int>();

                for (int j = 0; j < _clustersCount; j++)
                {
                    float minDistance = float.NaN;
                    int cluster = -1;
                    for (int k = 0; k < _clustersCount; k++)
                    {
                        if (finalDistance[i, k] < minDistance || float.IsNaN(minDistance))
                        {
                            minDistance = finalDistance[i, k];
                            cluster = k;
                        }
                    }
                    finalDistance[i, cluster] = float.PositiveInfinity;
                    clubsDistance[i].Add(cluster);
                }
            }

            for (int i = 0; i < _clubs.Count; i++)
            {
                int j = 0;
                bool ok = false;
                while (!ok)
                {
                    if (res[clubsDistance[i][j]].Count < clustersCapacity[clubsDistance[i][j]])
                    {
                        res[clubsDistance[i][j]].Add(_clubs[i]);
                        ok = true;
                    }
                    j++;
                }
            }

            //Foreach clubs to dispatch (like ultramarines clubs), we places them in hats starting from the lasts (hats with at least one club missing)
            int clusterCounter = _clustersCount - 1;
            foreach (Club c in _clubsToDispatch)
            {
                res[clusterCounter].Add(c);
                clusterCounter = clusterCounter == 0 ? _clustersCount - 1 : clusterCounter - 1;
            }

            return res;
        }

        public KMeansClustering(List<Club> clubs, int clustersCount, List<Club> noGeographicsClubsToDispatch)
        {
            _clubs = clubs;
            _clubsToDispatch = noGeographicsClubsToDispatch;
            _clustersCount = clustersCount;
        }

    }
}
