using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectYahu.Core;
using ProjectYahu.Models;
using ProjectYahu.Services;

namespace ProjectYahu.Core
{
    public class MarketOrchestrator : IDisposable
    {
        private readonly UniversalisClient _apiClient;
        private readonly InventoryScanner _inventoryScanner;
        private readonly ConfigurationService _config;

        public MarketOrchestrator(
            UniversalisClient apiClient,
            InventoryScanner inventoryScanner,
            ConfigurationService config)
        {
            _apiClient = apiClient;
            _inventoryScanner = inventoryScanner;
            _config = config;
        }

        /// <summary>
        /// Main entry point: Takes a list of inventory items, checks against Sell List,
        /// fetches market data, and returns actionable Listing Plans.
        /// </summary>
        public async Task<List<ListingPlan>> AnalyzeAndPlan(List<InventoryItem> rawInventory, string worldName)
        {
            var plans = new List<ListingPlan>();

            // 1. Identify items to sell
            var itemsToProcess = _inventoryScanner.GetItemsInSellList(rawInventory);
            if (!itemsToProcess.Any()) return plans;

            // 2. Batch fetch market data (optional optimization: currently 1-by-1 for simplicity/robustness)
            // In a real plugin, use FetchMultipleItems() for efficiency.
            foreach (var item in itemsToProcess)
            {
                var plan = await AnalyzeItem(item, worldName);
                if (plan != null)
                {
                    plans.Add(plan);
                }
                
                // Be gentle with the API
                await Common.RandomHelper.DelayAsync(100, 300);
            }

            return plans;
        }

        private async Task<ListingPlan?> AnalyzeItem(InventoryItem item, string worldName)
        {
            var itemConfig = _config.GetItemConfig(item.ItemId);
            if (itemConfig == null) return null; // Should be handled by Scanner, but safety first.

            // Fetch Data
            var snapshot = await _apiClient.FetchMarketData(item.ItemId, worldName);
            if (snapshot == null)
            {
                return new ListingPlan
                {
                    Item = item,
                    Status = PlanStatus.NoData,
                    AnalysisLog = "Failed to fetch market data."
                };
            }

            // Strategy Calculation
            var strategy = ListingStrategy.DetermineBestListing(snapshot, itemConfig.MatchHqOnly);

            // Safety Checks
            if (strategy.TargetPrice < itemConfig.MinPrice)
            {
                return new ListingPlan
                {
                    Item = item,
                    Status = PlanStatus.PriceTooLow,
                    AnalysisLog = $"Market price {strategy.TargetPrice} below minimum {itemConfig.MinPrice}."
                };
            }

            // Check if market is too unstable (e.g. huge MAD)
            // This is implicitly handled by StrategyResult returning "FixedPrice" or a high price if data is bad,
            // but we can add explicit checks here if needed.

            // Determine Stack Splitting
            int stackSize = itemConfig.ForcedStackSize > 0 
                ? itemConfig.ForcedStackSize 
                : strategy.RecommendedStackSize;

            int totalStacks = item.Count / stackSize;
            if (totalStacks == 0 && item.Count > 0) totalStacks = 1; // Sell whatever remains if < stackSize
            
            // Cap total listings based on config
            if (totalStacks > itemConfig.MaxListings)
                totalStacks = itemConfig.MaxListings;

            return new ListingPlan
            {
                Item = item,
                TargetPrice = strategy.TargetPrice,
                StackSize = stackSize,
                TotalStacks = totalStacks,
                Strategy = strategy.Mode,
                Status = PlanStatus.Ready,
                AnalysisLog = strategy.Reason
            };
        }

        public void Dispose()
        {
            _apiClient?.Dispose();
        }
    }
}
