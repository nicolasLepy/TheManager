using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Session
    {

        #region GestionSingleton
        private static Session _instance = null;
        public static Session Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Session();
                return _instance;
            }
        }

        private Session()
        {
            _random = new Random();
        }

        #endregion


        private Random _random;

        public Partie Partie { get; set; }

        /// <summary>
        /// Nombre aléatoire appartenant à [min,max[
        /// </summary>
        /// <param name="min">Borne incluse</param>
        /// <param name="max">Borne excluse</param>
        /// <returns></returns>
        public int Random(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Retourne un nombre entre 0.0 et 1.0
        /// </summary>
        /// <returns></returns>
        public double Random()
        {
            return _random.NextDouble();
        }

    }
}