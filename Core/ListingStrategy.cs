using System;
using System.Collections.Generic;
using System.Linq;
using ProjectYahu.Models;

namespace ProjectYahu.Core
{
    public class ListingStrategy
    {
        public enum PricingMode
        {
            Match, // Matches lowest valid price to prioritize "Newest First" logic
            Undercut, // Undercuts by 1 gil for slow markets
            FixedPrice // Fallback to a fixed price if market is unpredictable
        }

        public record StrategyResult
        {
            public long TargetPrice { get; init; }
            public int RecommendedStackSize { get; init; }
            public PricingMode Mode { get; init; }
            public string Reason { get; init; } = string.Empty;
        }

        /// <summary>
        /// Analyzes a market snapshot and determines the best price and stack size.
        /// </summary>
        public static StrategyResult DetermineBestListing(MarketSnapshot snapshot, bool matchHqOnly = true)
        {
            // 1. Filter out 'bait' and manipulation.
            var validListings = MarketAnalysis.FilterOutliersMAD(snapshot.Listings);

            if (validListings.Count == 0)
            {
                return new StrategyResult
                {
                    TargetPrice = 999999, // Fallback if market is empty/manipulated
                    RecommendedStackSize = 1,
                    Mode = PricingMode.FixedPrice,
                    Reason = "No valid listings found after outlier filtering."
                };
            }

            // 2. Identify target price from valid listings.
            var targetListings = matchHqOnly 
                ? validListings.Where(l => l.IsHq).ToList() 
                : validListings;
            
            // If no HQ listings, use NQ listings.
            if (targetListings.Count == 0) targetListings = validListings;

            var lowestValidListing = targetListings.OrderBy(l => l.Price).First();
            long basePrice = lowestValidListing.Price;

            // 3. Determine pricing mode based on velocity.
            // "Healthy" market > 5 sales/day; "Slow" market <= 5 sales/day.
            PricingMode mode = snapshot.SalesVelocityPerDay > 5.0 ? PricingMode.Match : PricingMode.Undercut;
            long targetPrice = mode == PricingMode.Match ? basePrice : basePrice - 1;

            // 4. Determine stack size optimization.
            int recommendedStack = OptimizeStackSize(snapshot.SalesVelocityPerDay);

            return new StrategyResult
            {
                TargetPrice = targetPrice,
                RecommendedStackSize = recommendedStack,
                Mode = mode,
                Reason = $"Velocity {snapshot.SalesVelocityPerDay:F1} sales/day leads to {mode} strategy."
            };
        }

        /// <summary>
        /// Heuristic for stack size optimization based on sales velocity.
        /// </summary>
        private static int OptimizeStackSize(double velocity)
        {
            if (velocity < 1.0) return 1;    // Very slow: sell one by one
            if (velocity < 5.0) return 10;   // Moderate: small stacks
            if (velocity < 20.0) return 30;  // Fast: medium stacks
            return 99;                       // Extremely fast: bulk (common mats)
        }
    }
}
