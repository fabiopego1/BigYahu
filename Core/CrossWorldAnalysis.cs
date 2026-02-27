using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectYahu.Models;

namespace ProjectYahu.Core
{
    public class CrossWorldAnalysis
    {
        private readonly UniversalisClient _client;

        public CrossWorldAnalysis(UniversalisClient client)
        {
            _client = client;
        }

        public record CrossWorldResult
        {
            public string BestBuyWorld { get; init; } = string.Empty;
            public long BestBuyPrice { get; init; }
            public string BestSellWorld { get; init; } = string.Empty;
            public long BestSellPrice { get; init; }
            public double ProfitMargin { get; init; }
            public string Recommendation { get; init; } = string.Empty;
        }

        /// <summary>
        /// Analyzes an item across the entire Data Center to find arbitrage or best selling world.
        /// </summary>
        public async Task<CrossWorldResult> AnalyzeDataCenter(uint itemId, string dataCenterName)
        {
            // 1. Fetch data for all worlds in DC
            var worldSnapshots = await _client.FetchDataCenterData(itemId, dataCenterName);
            if (worldSnapshots.Count == 0) return new CrossWorldResult { Recommendation = "No data found." };

            // 2. Find cheapest listing across all worlds (Best Buy)
            var allListings = worldSnapshots.Values.SelectMany(s => s.Listings).ToList();
            var validListings = MarketAnalysis.FilterOutliersMAD(allListings); // Filter bait globally? Or per world? Globally is safer for arbitrage.

            if (validListings.Count == 0) return new CrossWorldResult { Recommendation = "No valid listings in DC." };

            var cheapestListing = validListings.OrderBy(l => l.Price).First();
            string buyWorld = cheapestListing.WorldName;
            long buyPrice = cheapestListing.Price;

            // 3. Find most expensive world to sell (Best Sell)
            // Strategy: Find world with highest Minimum Price (that has decent velocity)
            string bestSellWorld = "";
            long maxMinPrice = 0;
            
            foreach (var kvp in worldSnapshots)
            {
                var worldName = kvp.Key;
                var snapshot = kvp.Value;
                
                // Skip if no listings or very low velocity
                if (snapshot.Listings.Count == 0 || snapshot.SalesVelocityPerDay < 1.0) continue;

                var worldValidListings = MarketAnalysis.FilterOutliersMAD(snapshot.Listings);
                if (worldValidListings.Count == 0) continue;
                
                long minPriceInWorld = worldValidListings.Min(l => l.Price);
                
                if (minPriceInWorld > maxMinPrice)
                {
                    maxMinPrice = minPriceInWorld;
                    bestSellWorld = worldName;
                }
            }

            if (string.IsNullOrEmpty(bestSellWorld))
            {
                bestSellWorld = buyWorld;
                maxMinPrice = buyPrice;
            }

            // 4. Calculate potential profit
            long profit = maxMinPrice - buyPrice;
            double margin = (double)profit / buyPrice;

            return new CrossWorldResult
            {
                BestBuyWorld = buyWorld,
                BestBuyPrice = buyPrice,
                BestSellWorld = bestSellWorld,
                BestSellPrice = maxMinPrice,
                ProfitMargin = margin,
                Recommendation = margin > 0.2 
                    ? $"Buy on {buyWorld} ({buyPrice}), Sell on {bestSellWorld} ({maxMinPrice})" 
                    : "No significant arbitrage opportunity."
            };
        }
    }
}
