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
        [PluginService] public static IClientState ClientState { get; private set; } = null!; // Needed for World check

        // --- Project Yahu Services ---
        private ConfigurationService ConfigService { get; init; }
        private MarketOrchestrator Orchestrator { get; init; }
        private UniversalisClient Universalis { get; init; }
        private InventoryScanner Scanner { get; init; }
        private GameInventoryService InventoryService { get; init; }
        private CrossWorldAnalysis CrossWorld { get; init; }
        
        // --- UI ---
        private WindowSystem WindowSystem = new("ProjectYahu");
        private UserInterface UI { get; init; }

        public Plugin()
        {
            // 1. Initialize Services
            this.ConfigService = new ConfigurationService(PluginInterface.GetPluginConfigDirectory());
            this.Universalis = new UniversalisClient();
            this.Scanner = new InventoryScanner(this.ConfigService); 
            this.InventoryService = new GameInventoryService(DataManager);
            this.Orchestrator = new MarketOrchestrator(this.Universalis, this.Scanner, this.ConfigService);
            this.CrossWorld = new CrossWorldAnalysis(this.Universalis);

            // 2. Initialize UI
            this.UI = new UserInterface(this.ConfigService, this.Orchestrator, this.CrossWorld);
            this.UI.OnRequestAnalysis += RunAnalysisFromUI;
            this.WindowSystem.AddWindow(this.UI);

            // 3. Register Commands & Events
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the Project Yahu interface."
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            this.UI.Dispose();
            CommandManager.RemoveHandler(CommandName);
            this.Universalis.Dispose();
            this.Orchestrator.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // Toggle the UI window
            this.UI.IsOpen = !this.UI.IsOpen;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        private void DrawConfigUI()
        {
            this.UI.IsOpen = true;
        }

        private async void RunAnalysisFromUI() 
        {
            // 1. Check World
            var localPlayer = ClientState.LocalPlayer;
            if (localPlayer == null) 
            {
                ChatGui.Print("[Project Yahu] Please log in first.");
                UI.SetIsAnalyzing(false);
                return;
            }

            var currentWorld = localPlayer.CurrentWorld.GameData?.Name.ToString() ?? "Cactuar"; // Fallback if null
            
            // 2. Get Inventory
            var realInventory = InventoryService.GetAllInventoryItems();
            
            if (realInventory.Count == 0)
            {
                ChatGui.Print("[Project Yahu] Inventory scan returned 0 items.");
                UI.SetIsAnalyzing(false);
                return;
            }

            // 3. Run Logic
            var plans = await Orchestrator.AnalyzeAndPlan(realInventory, currentWorld);

            // 4. Update UI
            UI.SetPlans(plans);
            
            if (plans.Count == 0)
            {
                ChatGui.Print("[Project Yahu] Analysis complete. No items from Sell List found.");
            }
            else
            {
                ChatGui.Print($"[Project Yahu] Analysis complete. Found {plans.Count} actionable items.");
            }
        }
    }
}
