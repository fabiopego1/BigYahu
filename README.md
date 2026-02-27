# Project Yahu - The Quiet Wealth Architect

Project Yahu is a powerful Dalamud plugin designed to automate market board pricing and identify cross-world arbitrage opportunities using "Quiet Wealth" tactics.

**Status:** 🟢 **Release Candidate 1**
-   **Full UI:** Now includes a dedicated ImGui window.
-   **Real Data:** Scans your actual inventory.
-   **Cross-World:** Finds profit across the entire Data Center.

## 🚀 Installation Guide

### 1. Prerequisites
-   **Visual Studio 2022 Community** (with .NET Desktop Development).
-   **XIVLauncher** (with Dalamud enabled).

### 2. Build & Install
1.  **Clone** this repository.
2.  Open `ProjectYahu.csproj` in Visual Studio.
3.  **Build Solution** (F6).
4.  In FFXIV, type `/xlsettings` -> **Experimental**.
5.  Add the path to your built `ProjectYahu.dll`.
6.  Click **Save**.

### 3. Usage
Type `/yahu` in chat to open the interface.

#### Tab 1: Market Analysis
-   **Goal:** Optimize prices for items you already own.
-   **How:** 
    1.  Ensure items you want to sell are in your `Sell List` (see Configuration).
    2.  Click **"Start Analysis"**.
    3.  Review the table:
        -   **Action:** Match, Undercut, or Fixed Price.
        -   **Price:** The calculated optimal price.
        -   **Stack:** Recommended stack size based on sales velocity.

#### Tab 2: Cross-World Arbitrage
-   **Goal:** Find items cheap on other servers to resell on yours.
-   **How:**
    1.  Enter your **Data Center** (e.g., Aether).
    2.  Click **"Scan DC"**.
    3.  Review the table for green rows (High Margin > 20%).

#### Tab 3: Configuration
-   **Sell List:** Add items here by ID to track them.
-   **Min Price:** Safety floor to prevent selling at a loss.
-   **Target Margin:** Desired profit percentage.

## 🧠 Core Features

-   **Real Inventory Scanning:** Uses `FFXIVClientStructs` to read your bag contents.
-   **Outlier Detection (MAD):** Filters out market manipulation/bait.
-   **Velocity Analysis:** Checks sales history to recommend stack sizes.
-   **Cross-World Comparison:** Fetches data for all worlds in your DC to find the best market.

## ⚠️ Disclaimer
This tool is for market analysis. Automating the actual posting process without user input may violate the Final Fantasy XIV Terms of Service. Use the data to make informed decisions manually.
