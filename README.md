# Project Yahu - Setup & Installation Guide

This guide will walk you through taking the **Project Yahu Core Logic** and turning it into a fully functional Dalamud plugin running inside your Final Fantasy XIV game.

## 🛠 Prerequisites (What you need installed)

Before you start, ensure you have the following installed on your computer:

1.  **Visual Studio 2022 Community Edition** (Free)
    -   Download: [visualstudio.microsoft.com](https://visualstudio.microsoft.com/vs/community/)
    -   **Important:** During installation, select the **.NET desktop development** workload.
2.  **git**
    -   Download: [git-scm.com](https://git-scm.com/downloads)
3.  **XIVLauncher (Dalamud)**
    -   You must be launching FFXIV through XIVLauncher with Dalamud enabled.
    -   Download: [goatcorp.github.io](https://goatcorp.github.io/)

---

## 🚀 Step 1: Create the Plugin Project

We need a "shell" project to hold our core logic.

1.  Open **PowerShell** or **Command Prompt**.
2.  Install the Dalamud templates:
    ```bash
    dotnet new install GoatCorp.Dalamud.Templates
    ```
3.  Navigate to where you want your project (e.g., `C:\Projects`):
    ```bash
    cd C:\Projects
    ```
4.  Create the new plugin:
    ```bash
    dotnet new dalamud -n ProjectYahu
    ```
5.  Open the newly created folder:
    ```bash
    cd ProjectYahu
    ```

---

## 📥 Step 2: Add the Core Logic

Now we will inject the "Quiet Wealth" logic into this empty plugin.

1.  **Clone this repository** (or download the ZIP) if you haven't already:
    ```bash
    git clone https://github.com/fabiopego1/BigYahu.git
    ```
    *(If you downloaded the ZIP, extract it somewhere temporary)*

2.  **Copy the Files:**
    -   Open the `BigYahu` folder you just downloaded.
    -   Select the **Core**, **Models**, **Services**, and **Common** folders.
    -   **Copy** them.
    -   Go to your new `C:\Projects\ProjectYahu` folder.
    -   **Paste** them there.

3.  **Install Dependencies:**
    -   Open your project in Visual Studio (double-click `ProjectYahu.sln`).
    -   Right-click the **ProjectYahu** project in the "Solution Explorer" (right side).
    -   Select **Manage NuGet Packages**.
    -   Search for `System.Text.Json`.
    -   Click **Install**.
    -   **CRITICAL:** Also search for and install `DalamudPackager`.
    -   Ensure your project file (`.csproj`) references the Dalamud API. (The template usually handles this, but if `IDalamudPlugin` is red, you need to add the reference).

---

## 🔌 Step 3: Wire It Up (The Code)

Now we need to tell the plugin to use our logic.

1.  **Open `Plugin.cs`** in Visual Studio.
2.  **Replace the entire content** with this code:

    ```csharp
    using Dalamud.Game.Command;
    using Dalamud.IoC;
    using Dalamud.Plugin;
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

            private DalamudPluginInterface PluginInterface { get; init; }
            private CommandManager CommandManager { get; init; }
            private ConfigurationService ConfigService { get; init; }
            private MarketOrchestrator Orchestrator { get; init; }
            private UniversalisClient Universalis { get; init; }
            private InventoryScanner Scanner { get; init; }

            public Plugin(
                [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
                [RequiredVersion("1.0")] CommandManager commandManager)
            {
                this.PluginInterface = pluginInterface;
                this.CommandManager = commandManager;

                // 1. Initialize Our Services
                this.ConfigService = new ConfigurationService(this.PluginInterface.GetPluginConfigDirectory());
                this.Universalis = new UniversalisClient();
                this.Scanner = new InventoryScanner(this.ConfigService); // Needs to be hooked to real inventory later!
                this.Orchestrator = new MarketOrchestrator(this.Universalis, this.Scanner, this.ConfigService);

                // 2. Register Commands
                this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
                {
                    HelpMessage = "Opens the Project Yahu interface."
                });

                this.PluginInterface.UiBuilder.Draw += DrawUI;
            }

            public void Dispose()
            {
                this.CommandManager.RemoveHandler(CommandName);
                this.Universalis.Dispose();
                this.Orchestrator.Dispose();
            }

            private void OnCommand(string command, string args)
            {
                // For now, let's just run a simulation in the chat log
                RunAnalysis();
            }

            private void DrawUI()
            {
                // UI Code would go here (WindowSystem)
            }

            private async void RunAnalysis() 
            {
                // MOCK INVENTORY FOR TESTING
                // In a real release, you'd use: Dalamud.Game.ClientState.Inventory
                var mockInventory = new System.Collections.Generic.List<InventoryItem> 
                {
                    new InventoryItem { ItemId = 5111, Name = "Iron Ore (Test)", Count = 99 },
                    new InventoryItem { ItemId = 5530, Name = "Coke (Test)", Count = 30 }
                };

                // Add test items to config so they are recognized
                ConfigService.AddToSellList(new ItemConfig { ItemId = 5111, MinPrice = 100 });
                ConfigService.AddToSellList(new ItemConfig { ItemId = 5530, MinPrice = 200 });

                Dalamud.Logging.PluginLog.Log("Project Yahu: Starting Market Analysis...");
                
                var plans = await Orchestrator.AnalyzeAndPlan(mockInventory, "Cactuar"); // Change 'Cactuar' to your world!

                foreach(var plan in plans)
                {
                    Dalamud.Logging.PluginLog.Log($"[YAHU] {plan.ToString()}");
                }
            }
        }
    }
    ```

3.  **Fix Missing References:**
    -   If red squiggles appear, hover over them and press `Alt + Enter`, then select `using ...`.
    -   Ensure `InventoryScanner` accepts `ConfigurationService` in its constructor (you might need to update `Core/InventoryScanner.cs` to match the usage if it changed).

---

## 🏗 Step 4: Build & Install

1.  **Build the Project:**
    -   In Visual Studio, press `Ctrl + Shift + B` (or right-click Project -> Build).
    -   Look at the "Output" window at the bottom. It should say **Build: 1 succeeded**.
    -   Note the path where the `.dll` was created (usually `bin\x64\Debug\ProjectYahu.dll`).

2.  **Load in FFXIV:**
    -   Launch FFXIV via XIVLauncher.
    -   In-game, type `/xlsettings` to open Dalamud settings.
    -   Go to the **Experimental** tab.
    -   Under **Dev Plugins**, add the full path to your `ProjectYahu.dll`.
    -   Click **Save and Close**.

3.  **Test It:**
    -   Type `/yahu` in the chat.
    -   Open the **Dalamud Log** (`/xllog`).
    -   You should see:
        ```
        [Project Yahu] Starting Market Analysis...
        [YAHU] [LIST] Iron Ore (Test): 1x stacks of 99 @ 150 gil (Match)
        ```

---

## ⚠️ Important Notes

*   **Mock Data:** The current setup uses a "Mock Inventory" (fake items) so you can test safely without needing to be at a retainer bell.
*   **Real Data:** To use your real inventory, you will need to replace the `mockInventory` list in `Plugin.cs` with calls to `Dalamud.Game.ClientState.Inventory`.
*   **Safety:** Always test with `MinPrice` set in your config to prevent selling items too cheaply!

## 🤝 Contributing

Fork this repository, create a branch, and submit a Pull Request!
