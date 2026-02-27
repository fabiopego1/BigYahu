using System;
using System.Collections.Generic;
using System.Linq;
using ProjectYahu.Models;

namespace ProjectYahu.Core
{
    public static class MarketAnalysis
    {
        /// <summary>
        /// Filters out 'bait' listings using Median Absolute Deviation (MAD).
        /// MAD is more robust against large outliers than standard deviation.
        /// </summary>
        /// <param name="listings">Original market listings</param>
        /// <param name="threshold">Typically 2.5 to 3.5; lower is more aggressive.</param>
        /// <returns>Filtered list of valid listings</returns>
        public static List<Listing> FilterOutliersMAD(List<Listing> listings, double threshold = 3.5)
        {
            if (listings.Count < 3) return listings;

            var prices = listings.Select(l => (double)l.Price).OrderBy(p => p).ToList();
            double median = CalculateMedian(prices);

            // Absolute deviations from median
            var deviations = prices.Select(p => Math.Abs(p - median)).OrderBy(d => d).ToList();
            double mad = CalculateMedian(deviations);

            // If MAD is 0, all prices are the same, or most are the same. 
            // We can't use MAD effectively; return original.
            if (Math.Abs(mad) < 0.0001) return listings;

            return listings.Where(l =>
            {
                // Modified Z-score calculation
                // Constant 0.6745 is used to make MAD a consistent estimator of standard deviation
                double zScore = 0.6745 * Math.Abs(l.Price - median) / mad;
                return zScore <= threshold;
            }).ToList();
        }

        /// <summary>
        /// Alternative outlier detection using Interquartile Range (IQR).
        /// Often used for more 'standard' distribution cleanup.
        /// </summary>
        public static List<Listing> FilterOutliersIQR(List<Listing> listings)
        {
            if (listings.Count < 4) return listings;

            var prices = listings.Select(l => (double)l.Price).OrderBy(p => p).ToList();
            int n = prices.Count;

            double q1 = CalculateMedian(prices.Take(n / 2).ToList());
            double q3 = CalculateMedian(prices.Skip((n + 1) / 2).ToList());
            double iqr = q3 - q1;

            double lowerBound = q1 - (1.5 * iqr);
            double upperBound = q3 + (1.5 * iqr);

            return listings.Where(l => l.Price >= lowerBound && l.Price <= upperBound).ToList();
        }

        private static double CalculateMedian(List<double> values)
        {
            if (values.Count == 0) return 0;
            int middle = values.Count / 2;
            if (values.Count % 2 == 0)
                return (values[middle - 1] + values[middle]) / 2.0;
            return values[middle];
        }

        /// <summary>
        /// Calculates sales velocity based on recent sale history.
        /// Returns sales per 24-hour period.
        /// </summary>
        public static double CalculateVelocity(List<SaleHistory> sales, int daysToAnalyze = 7)
        {
            if (sales.Count == 0) return 0;

            var cutoff = DateTime.UtcNow.AddDays(-daysToAnalyze);
            var recentSales = sales.Where(s => s.SaleTime >= cutoff).ToList();

            if (recentSales.Count == 0) return 0;

            // Simple average: total sales in window / window duration
            return (double)recentSales.Sum(s => s.Quantity) / daysToAnalyze;
        }
    }
}
