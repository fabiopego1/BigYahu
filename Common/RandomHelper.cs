using System;
using System.Threading.Tasks;

namespace ProjectYahu.Common
{
    public static class RandomHelper
    {
        private static readonly Random _random = new();

        /// <summary>
        /// Generates a randomized delay between min and max milliseconds.
        /// Useful for human-like interactions.
        /// </summary>
        public static async Task DelayAsync(int minMs = 800, int maxMs = 2500)
        {
            int delay = _random.Next(minMs, maxMs);
            await Task.Delay(delay);
        }

        /// <summary>
        /// Randomizes a price slightly within a very small range (0.1% or similar)
        /// if the user wants to further avoid pattern detection.
        /// </summary>
        public static long RandomizePrice(long basePrice, double variance = 0.0)
        {
            if (variance <= 0.0) return basePrice;
            double factor = 1.0 + (_random.NextDouble() * 2.0 - 1.0) * variance;
            return (long)Math.Round(basePrice * factor);
        }
    }
}
