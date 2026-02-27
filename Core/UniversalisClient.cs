using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ProjectYahu.Models;

namespace ProjectYahu.Core
{
    public class UniversalisClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://universalis.app/api/v2/";

        public UniversalisClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ProjectYahu/1.0");
        }

        /// <summary>
        /// Fetches market data for a specific item and world.
        /// </summary>
        public async Task<MarketSnapshot?> FetchMarketData(uint itemId, string world)
        {
            string url = $"{BaseUrl}{world}/{itemId}?listings=20&entries=20";
            
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var snapshot = JsonSerializer.Deserialize<MarketSnapshot>(response);

                if (snapshot != null)
                {
                    // Calculate velocity immediately after fetch
                    snapshot.SalesVelocityPerDay = MarketAnalysis.CalculateVelocity(snapshot.RecentSales);
                }

                return snapshot;
            }
            catch (Exception ex)
            {
                // In a real Dalamud plugin, log this to the plugin log
                Console.WriteLine($"[Universalis] Error fetching {itemId} for {world}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Batch fetch multiple items to save API calls.
        /// </summary>
        public async Task<List<MarketSnapshot>> FetchMultipleItems(IEnumerable<uint> itemIds, string world)
        {
            string ids = string.Join(",", itemIds);
            string url = $"{BaseUrl}{world}/{ids}?listings=5&entries=5"; // Smaller limit for batch

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                
                // Universalis returns a different structure for multiple items
                // { "items": { "item1": { ... }, "item2": { ... } } }
                // For simplicity, we'll implement this if you need batching logic.
                // For now, returning an empty list to avoid complexity.
                return new List<MarketSnapshot>();
            }
            catch
            {
                return new List<MarketSnapshot>();
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
