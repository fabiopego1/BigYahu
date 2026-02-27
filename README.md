# Project Yahu - The Quiet Wealth Architect

Project Yahu is a fully functional **Dalamud Plugin Core** designed to automate market board pricing with "Quiet Wealth" tactics.

**Status:** 🟢 **Real Game Data Ready**
This codebase now includes a full implementation of `GameInventoryService` using `FFXIVClientStructs`, meaning it can read your actual in-game inventory without any mocking.

## 🚀 Installation Guide (The "Dummy-Proof" Version)

Follow these steps to get Project Yahu running in your game:

### 1. Prerequisites
-   **Visual Studio 2022 Community** (with .NET Desktop Development workload).
-   **XIVLauncher** (with Dalamud enabled).

### 2. Set Up the Project
1.  **Clone or Download** this repository.
2.  Open the folder in **Visual Studio** (Double-click `ProjectYahu.csproj` or open Folder).
    *   *Note: If you don't see a solution file (.sln), you can just open the .csproj directly.*

### 3. Verify References
The included `ProjectYahu.csproj` is already configured to pull the necessary dependencies:
-   `DalamudPackager`
-   `Newtonsoft.Json`
-   `FFXIVClientStructs` (for reading memory)
-   `Dalamud` & `Lumina` (from your local XIVLauncher installation)

**Important:** You might need to adjust the `<DalamudLibPath>` in `ProjectYahu.csproj` if your XIVLauncher is installed in a non-standard location.
Default: `$(appdata)\XIVLauncher\addon\Hooks\dev\`

### 4. Build
1.  Select **Debug** or **Release** configuration (Top bar).
2.  Press **F6** or `Build -> Build Solution`.
3.  Check the "Output" window. It should say **Build Succeeded**.

### 5. Load in FFXIV
1.  Launch the game.
2.  Type `/xlsettings` -> **Experimental**.
3.  Add the path to your built DLL (e.g., `.../ProjectYahu/bin/Debug/net8.0-windows/ProjectYahu.dll`).
4.  Click **Save**.

### 6. Usage
1.  **Configure:** Edit `Models/Configuration.cs` or the generated JSON to add items to your "Sell List". (Currently, no UI for adding items, you must edit the config file or code).
2.  **Run:** Type `/yahu` in chat.
3.  **Check Log:** Open `/xllog` to see the analysis:
    ```
    [Project Yahu] Scanned 150 items in your bags.
    [YAHU] [LIST] Iron Ore: 3x stacks of 99 @ 150 gil (Match)
    ```

## 🧠 Core Features

-   **Real Inventory Scanning:** Uses `FFXIVClientStructs` to read your bag contents.
-   **Outlier Detection (MAD):** Filters out market manipulation/bait.
-   **Velocity Analysis:** Checks sales history to recommend stack sizes.
-   **Safe Pricing:** Never lists below your defined `MinPrice`.

## ⚠️ Disclaimer
This is a **developer tool/core library**. While functional, it lacks a GUI for configuration. It is intended for developers who want to build upon the "Quiet Wealth" algorithm.
