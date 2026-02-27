using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;
using ProjectYahu.Core;
using ProjectYahu.Services;
using ProjectYahu.Models;
using System.Linq;

namespace ProjectYahu
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Project Yahu";
        private const string CommandName = "/yahu";

        // --- Dalamud Services ---
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;

        // --- Project Yahu Services ---
        private ConfigurationService ConfigService { get; init; }
        private MarketOrchestrator Orchestrator { get; init; }
        private UniversalisClient Universalis { get; init; }
        private InventoryScanner Scanner { get; init; }
        private GameInventoryService InventoryService { get; init; } // New service!

        public Plugin()
        {
            // 1. Initialize Our Services
            this.ConfigService = new ConfigurationService(PluginInterface.GetPluginConfigDirectory());
            this.Universalis = new UniversalisClient();
            this.Scanner = new InventoryScanner(this.ConfigService); 
            this.InventoryService = new GameInventoryService(DataManager); // Pass DataManager here!
            this.Orchestrator = new MarketOrchestrator(this.Universalis, this.Scanner, this.ConfigService);

            // 2. Register Commands
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the Project Yahu analysis (Check Chat Log)."
            });
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler(CommandName);
            this.Universalis.Dispose();
            this.Orchestrator.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            RunAnalysis();
        }

        private async void RunAnalysis() 
        {
            ChatGui.Print("[Project Yahu] Starting REAL Market Analysis...");
            
            // 1. Get REAL Inventory
            var realInventory = InventoryService.GetAllInventoryItems();
            ChatGui.Print($"[Project Yahu] Scanned {realInventory.Count} items in your bags.");

            if (realInventory.Count == 0)
            {
                ChatGui.Print("[Project Yahu] Inventory appears empty or scan failed.");
                return;
            }

            // 2. Run Logic
            // Note: Update 'Cactuar' to dynamically fetch current world from ClientState later!
            var plans = await Orchestrator.AnalyzeAndPlan(realInventory, "Cactuar");

            if (plans.Count == 0)
            {
                ChatGui.Print("[Project Yahu] No items from your 'Sell List' were found in inventory.");
                return;
            }

            foreach(var plan in plans)
            {
                ChatGui.Print($"[YAHU] {plan.ToString()}");
            }
        }
    }
}
