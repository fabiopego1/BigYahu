using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectYahu.Core;
using ProjectYahu.Models;
using ProjectYahu.Services;
using ProjectYahu.Common;

namespace ProjectYahu
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Project Yahu Orchestrator Simulation...");

            // 1. Setup Services
            var config = new ConfigurationService("project_yahu_test.json");
            var inventoryScanner = new InventoryScanner(config);
            var apiClient = new UniversalisClient(); // Uses real API, might fail if no internet/item specific
            
            // 2. Add some test items to config (Sell List)
            // Example: Iron Ore (ItemId: 5111) - Common item
            // Example: Coke (ItemId: 5530)
            config.AddToSellList(new ItemConfig { 
                ItemId = 5111, 
                ItemName = "Iron Ore", 
                MinPrice = 100, 
                TargetMargin = 0.15 
            });
            config.AddToSellList(new ItemConfig { 
                ItemId = 5530, 
                ItemName = "Coke", 
                MinPrice = 200, 
                ForcedStackSize = 99 
            });

            // 3. Mock Inventory Scan
            var mockInventory = new List<InventoryItem>
            {
                new InventoryItem { ItemId = 5111, Name = "Iron Ore", Count = 250, Slot = 0 },
                new InventoryItem { ItemId = 5530, Name = "Coke", Count = 50, Slot = 1 },
                new InventoryItem { ItemId = 9999, Name = "Junk Item", Count = 1, Slot = 2 } // Not in Sell List
            };

            // 4. Run Orchestrator
            using (var orchestrator = new MarketOrchestrator(apiClient, inventoryScanner, config))
            {
                Console.WriteLine("Scanning inventory and fetching market data...");
                var plans = await orchestrator.AnalyzeAndPlan(mockInventory, "Cactuar"); // Assuming Cactuar as world

                Console.WriteLine("
--- Proposed Listing Plans ---");
                foreach (var plan in plans)
                {
                    Console.WriteLine(plan.ToString());
                    Console.WriteLine($"   Reason: {plan.AnalysisLog}");
                    if (plan.Status == PlanStatus.PriceTooLow)
                    {
                        Console.WriteLine($"   WARNING: Market price is below minimum threshold!");
                    }
                }
            }

            Console.WriteLine("
Simulation Complete.");
        }
    }
}
