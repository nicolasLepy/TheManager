using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Session
    {

        #region SingletonManagement
        private static Session _instance;
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

        public Game Game { get; set; }

        /// <summary>
        /// Random number in [min,max[
        /// </summary>
        /// <returns></returns>
        public int Random(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Return a number between 0.0 and 1.0
        /// </summary>
        /// <returns></returns>
        public double Random()
        {
            return _random.NextDouble();
        }

    }
}