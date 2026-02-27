using System;
using System.Collections.Generic;

namespace ProjectYahu.Models
{
    public class ItemConfig
    {
        public uint ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        
        // Minimum price to never go below (Safety check)
        public long MinPrice { get; set; } = 100;
        
        // Target Profit Margin (e.g., 20%) - useful for crafting
        public double TargetMargin { get; set; } = 0.20;
        
        // Custom Stack Size (if 0, use calculated velocity logic)
        public int ForcedStackSize { get; set; } = 0;
        
        // Whether we only want to match HQ prices
        public bool MatchHqOnly { get; set; } = true;
        
        // Maximum number of stacks to have listed at once
        public int MaxListings { get; set; } = 2;
    }

    public class PluginConfig
    {
        public int Version { get; set; } = 0;
        
        // Global delay multipliers for human-like behavior
        public double DelayMultiplier { get; set; } = 1.0;
        
        // The list of items we are actively monitoring
        public List<ItemConfig> SellList { get; set; } = new();
        
        // Default outlier threshold (3.5 is the MAD standard)
        public double OutlierThreshold { get; set; } = 3.5;
    }
}
