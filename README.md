# Project Yahu - The Quiet Wealth Architect

Project Yahu is a specialized core library for a Dalamud (FFXIV) plugin designed to automate and optimize market board listings with a focus on "Quiet Wealth." Instead of aggressive undercutting wars, Yahu employs statistical analysis to identify fair market value, filter out "bait" listings, and maximize profit margins through intelligent pricing and stack optimization.

**Disclaimer:** This tool is intended for educational purposes and personal use to assist in market analysis. Automating game actions (like posting listings) without user input may violate the Final Fantasy XIV Terms of Service. Use responsibly.

## 🧠 Core Philosophy: "Quiet Wealth"

Most market board bots engage in a "race to the bottom," undercutting by 1 gil every few minutes. This crashes markets and attracts attention.

**Project Yahu takes a different approach:**
1.  **Precision Pricing:** We don't just undercut; we analyze the distribution of prices.
2.  **Outlier Filtering:** Using **Median Absolute Deviation (MAD)**, we ignore "bait" listings (ridiculously low prices designed to trick auto-undercutters) and "market manipulation" (ridiculously high prices).
3.  **Liquidity Over Speed:** We calculate sales velocity to determine if an item sells fast enough to warrant matching the lowest price, or if we should undercut slightly.
4.  **Stack Optimization:** We analyze sales history to recommend optimal stack sizes (e.g., selling 99 ore vs. 3 stacks of 33 based on what actually sells).

## 🏗 Architecture

The project is structured as a standalone C# library that can be integrated into any Dalamud plugin or external tool.

### 1. `Core/MarketAnalysis.cs` (The Brain)
-   **MAD Outlier Detection:** Robust statistical filtering to ignore price anomalies.
-   **Velocity Calculation:** Determines sales per day based on recent history.

### 2. `Core/ListingStrategy.cs` (The Strategist)
-   **Pricing Logic:** Decides whether to Match, Undercut, or hold a Fixed Price.
-   **Stack Sizing:** Recommends optimal stack sizes based on velocity tiers.

### 3. `Core/UniversalisClient.cs` (The Eyes)
-   **API Integration:** Fetches real-time market data from Universalis.app.
-   **Data Mapping:** Converts JSON responses into our internal `MarketSnapshot` model.

### 4. `Core/MarketOrchestrator.cs` (The Conductor)
-   **Workflow Management:** Coordinates the Inventory Scan -> API Fetch -> Analysis -> Plan generation pipeline.
-   **Safety Checks:** Ensures no item is listed below the user-defined `MinPrice`.

## 🚀 Getting Started (Integration Guide)

This repository contains the **Core Logic**. To use this in a Dalamud plugin:

1.  **Copy the Files:**
    -   Copy the `Core/`, `Models/`, `Services/`, and `Common/` directories into your Dalamud plugin project.

2.  **Install Dependencies:**
    -   `System.Text.Json` (or replace with `Newtonsoft.Json` if preferred).

3.  **Hook into Dalamud:**
    In your main `IDalamudPlugin` class:

    ```csharp
    // 1. Initialize Services
    var configService = new ConfigurationService(PluginInterface.GetPluginConfigDirectory());
    var scanner = new InventoryScanner(configService); // You'll need to hook this to Dalamud's InventoryManager
    var client = new UniversalisClient();
    var orchestrator = new MarketOrchestrator(client, scanner, configService);

    // 2. Scan & Plan (e.g., on button click)
    var plans = await orchestrator.AnalyzeAndPlan(myInventoryItems, "Cactuar");

    // 3. Execute (Your UI Logic)
    foreach (var plan in plans) {
        if (plan.Status == PlanStatus.Ready) {
            // Use ClickLib or similar to execute the plan
            Log($"Listing {plan.Item.Name} for {plan.TargetPrice} gil.");
        }
    }
    ```

## 🛠 Testing

You can run the included `tests/verify_outliers.py` script to see the outlier detection algorithm in action with simulated data:

```bash
python3 tests/verify_outliers.py
```

## 📄 License

MIT License - See [LICENSE](LICENSE) for details.
