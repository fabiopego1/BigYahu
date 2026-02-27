using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProjectYahu.Models
{
    public record Listing
    {
        [JsonPropertyName("pricePerUnit")]
        public long Price { get; init; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; init; }
        [JsonPropertyName("hq")]
        public bool IsHq { get; init; }
        [JsonPropertyName("lastReviewTime")]
        public long LastReviewUnix { get; init; }
        [JsonPropertyName("retainerName")]
        public string RetainerName { get; init; } = string.Empty;
        
        [JsonPropertyName("worldName")]
        public string WorldName { get; init; } = string.Empty;
        
        [JsonPropertyName("worldID")]
        public uint WorldId { get; init; }

        public DateTime LastReviewTime => DateTimeOffset.FromUnixTimeSeconds(LastReviewUnix).UtcDateTime;
    }

    public record SaleHistory
    {
        [JsonPropertyName("pricePerUnit")]
        public long Price { get; init; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; init; }
        [JsonPropertyName("timestamp")]
        public long TimestampUnix { get; init; }
        [JsonPropertyName("hq")]
        public bool IsHq { get; init; }

        public DateTime SaleTime => DateTimeOffset.FromUnixTimeSeconds(TimestampUnix).UtcDateTime;
    }

    public record MarketSnapshot
    {
        [JsonPropertyName("itemID")]
        public uint ItemId { get; init; }
        [JsonPropertyName("worldName")]
        public string WorldName { get; init; } = string.Empty;
        [JsonPropertyName("listings")]
        public List<Listing> Listings { get; init; } = new();
        [JsonPropertyName("recentHistory")]
        public List<SaleHistory> RecentSales { get; init; } = new();
        
        // Calculated locally, not from raw JSON directly in the same way
        public double SalesVelocityPerDay { get; set; }
    }
}
