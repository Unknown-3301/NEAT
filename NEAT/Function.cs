using System;

namespace NEAT
{
    internal static class Function
    {
        /// <summary>
        /// Change value range from one to another
        /// </summary>
        /// <param name="Value">The value</param>
        /// <param name="MinValue">The minimum range for Value (Inclusive)</param>
        /// <param name="MaxValue">The maximum range for Value (Inclusive)</param>
        /// <param name="MinRange">The new minimum range for Value (Inclusive)</param>
        /// <param name="MaxRange">The new maximum range for Value (Inclusive)</param>
        /// <returns></returns>
        public static double Map(double Value, double MinValue, double MaxValue, double MinRange, double MaxRange)
        {
            return (((Value - MinValue) * (MaxRange - MinRange)) / (MaxValue - MinValue)) + MinRange;
        }
        /// <summary>
        /// Return normal distribution number between -StanDev and StanDev
        /// NOTE: there is no real range it just most numbers will be inside the range and small amount will be out
        /// </summary>
        /// <param name="Mean">The common number in range</param>
        /// <param name="StanDev">The positive of range</param>
        /// <param name="rand">Random seed</param>
        /// <returns></returns>
        public static double RandomGaussain(double Mean, double StanDev, Random rand)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randNormal = Mean + StanDev * randStdNormal;
            return randNormal;
        }
        /// <summary>
        /// Returns The number sign (-1 for any negative / 1 for any positive / 0 for 0)
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int Sign(double num)
        {
            if (num > 0)
                return 1;
            else if (num < 0)
                return -1;

            return 0;
        }
        #region public static int Paring(int[] k)
        /// <summary>
        /// Creates unique number by n numbers (be aware that the output will rise up super fast as the number of dimensions go up)
        /// <br>Source >> https://en.wikipedia.org/wiki/Pairing_function </br>
        /// </summary>
        /// <param name="k">Vector in n dimension</param>
        /// <returns></returns>
        public static ulong Paring(ulong[] k)
        {
            if (k.Length == 1)
                return k[0];
            else if (k.Length == 2)
                return ((k[0] + k[1]) * (k[0] + k[1] + 1) / 2) + k[1];
            else
                return Paring(k, k.Length);
        }
        private static ulong Paring(ulong[] k, int size)
        {
            if (size == 2)
                return ((k[0] + k[1]) * (k[0] + k[1] + 1) / 2) + k[1];
            else
                return Paring2D(Paring(k, size - 1), k[size - 1]);
        }
        public static ulong Paring2D(ulong x, ulong y)
        {
            return ((x + y) * (x + y + 1) / 2) + y;
        }
        #endregion

        /// <summary>
        /// This is from Sebastian Lague
        /// Source link: https://www.youtube.com/watch?v=lctXaT9pxA0
        /// </summary>
        /// <param name="x"></param>
        /// <param name="bias"></param>
        /// <returns></returns>
        public static double BiasFunction(double x, double bias)
        {
            double h = 1 - bias;
            double k = h * h * h;

            return (x * k) / (x * k - x + 1);
        }
    }
}