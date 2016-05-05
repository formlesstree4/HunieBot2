using System;
using System.Collections.Generic;

namespace HunieBot.BlackJack
{
    public static class Extensions
    {

        private static readonly Random _random = new MersenneTwister();

        public static void Shuffle<T>(this IList<T> list)
        {
            list.Shuffle(_random);
        }

        /// <summary>
        ///     Shuffle the list.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="array">Array to shuffle.</param>
        /// <remarks>
        ///     This is an implementation of the Fischer-Yates algorithm.
        /// </remarks>
        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            lock (random)
            {
                int n = list.Count;
                for (int i = 0; i < n; i++)
                {
                    // NextDouble returns a random number between 0 and 1.
                    int r = i + (int)(random.NextDouble() * (n - i));
                    T t = list[r];
                    list[r] = list[i];
                    list[i] = t;
                }
            }
        }

    }
}
