using System;
using System.Collections.Generic;
using System.Linq;
using ProjectYahu.Models;
using ProjectYahu.Services;

namespace ProjectYahu.Core
{
    public class InventoryItem
    {
        public uint ItemId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Count { get; init; }
        public bool IsHq { get; init; }
        public int Slot { get; init; }
    }

    /// <summary>
    /// Mock of the in-game inventory scanner.
    /// In Dalamud, this would use SeString or Item ID lookups.
    /// </summary>
    public class InventoryScanner
    {
        private readonly ConfigurationService _configService;

        public InventoryScanner(ConfigurationService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// Scans player inventory (and potentially armory/saddlebags)
        /// and returns only the items that are in the "Sell List".
        /// </summary>
        public List<InventoryItem> GetItemsInSellList(List<InventoryItem> allInGameItems)
        {
            var sellListIds = _configService.Config.SellList.Select(i => i.ItemId).ToHashSet();
            
            // Filter only items that match our Sell List
            var itemsToSell = allInGameItems
                .Where(item => sellListIds.Contains(item.ItemId))
                .ToList();

            return itemsToSell;
        }

        /// <summary>
        /// Checks if an item is eligible for listing based on profit margins.
        /// </summary>
        public bool IsEligible(uint itemId, long currentPrice)
        {
            var itemConfig = _configService.GetItemConfig(itemId);
            if (itemConfig == null) return false;

            // Safety check: Never list below our minimum price
            return currentPrice >= itemConfig.MinPrice;
        }
    }
}
