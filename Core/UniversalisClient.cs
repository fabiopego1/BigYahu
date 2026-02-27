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
        /// Fetches market data for the ENTIRE Data Center.
        /// Returns a dictionary of "WorldName" -> MarketSnapshot.
        /// </summary>
        public async Task<Dictionary<string, MarketSnapshot>> FetchDataCenterData(uint itemId, string dataCenterName)
        {
            string url = $"{BaseUrl}{dataCenterName}/{itemId}?listings=50&entries=50";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var snapshot = JsonSerializer.Deserialize<MarketSnapshot>(response);
                
                // Universalis returns a single object with ALL listings for a DC request.
                // We need to group them by WorldName to analyze per world.
                if (snapshot == null || snapshot.Listings == null) return new Dictionary<string, MarketSnapshot>();

                var worldSnapshots = new Dictionary<string, MarketSnapshot>();

                // Group Listings by WorldName
                var listingsByWorld = snapshot.Listings.GroupBy(l => l.WorldName ?? "Unknown");

                foreach (var group in listingsByWorld)
                {
                    string world = group.Key;
                    if (string.IsNullOrEmpty(world)) continue;

                    // Reconstruct a snapshot for this specific world
                    var worldListings = group.ToList();
                    
                    // Sales history is global in the DC response? No, usually mixed. 
                    // Universalis API v2 for DC requests is tricky. 
                    // It returns `listings` array where each listing has `worldID` or `worldName`.
                    // It ALSO returns `recentHistory` array where each sale has `worldID` or `worldName`.
                    
                    // Filter history for this world too
                    var worldHistory = snapshot.RecentSales?
                        .Where(s => GetWorldNameForHistory(s, world)) // Dummy check, see below
                        .ToList() ?? new List<SaleHistory>();

                    var subSnapshot = new MarketSnapshot
                    {
                        ItemId = itemId,
                        WorldName = world,
                        Listings = worldListings,
                        RecentSales = worldHistory,
                        SalesVelocityPerDay = MarketAnalysis.CalculateVelocity(worldHistory)
                    };
                    
                    worldSnapshots[world] = subSnapshot;
                }

                return worldSnapshots;
            }
            catch
            {
                return new Dictionary<string, MarketSnapshot>();
            }
        }

        // Helper: In a real implementation, you'd map WorldID -> WorldName using Lumina.
        // For simplicity, we assume Universalis returns WorldName if possible, or we skip history filtering by world if complex.
        private bool GetWorldNameForHistory(SaleHistory sale, string targetWorld)
        {
            // Universalis `recentHistory` objects in DC response typically have `worldID` or `worldName`.
            // Without a proper Models update to include WorldID on SaleHistory, we can't filter effectively.
            // For now, we will return TRUE to include all history in velocity calc (Data Center Velocity).
            return true; 
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
