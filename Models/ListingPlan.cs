using System;
using ProjectYahu.Core;

namespace ProjectYahu.Models
{
    public enum PlanStatus
    {
        Ready,          // Good to list
        PriceTooLow,    // Market price is below user's minimum
        MarketUnstable, // Too much volatility/bait
        NoData          // API failed
    }

    public record ListingPlan
    {
        public InventoryItem Item { get; init; }
        public long TargetPrice { get; init; }
        public int StackSize { get; init; }
        public int TotalStacks { get; init; }
        public ListingStrategy.PricingMode Strategy { get; init; }
        public PlanStatus Status { get; init; }
        public string AnalysisLog { get; init; } = string.Empty;

        public override string ToString()
        {
            if (Status != PlanStatus.Ready)
                return $"[SKIP] {Item.Name}: {Status} - {AnalysisLog}";
            
            return $"[LIST] {Item.Name}: {TotalStacks}x stacks of {StackSize} @ {TargetPrice:N0} gil ({Strategy})";
        }
    }
}
