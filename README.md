# Project Yahu Setup Guide

Project Yahu is a market analysis tool for Final Fantasy XIV that utilizes statistical methods to identify optimal pricing and cross-world arbitrage opportunities.

## Prerequisites

Ensure the following are installed before proceeding:

1. Visual Studio 2022 Community Edition
   - During installation, you must select the ".NET desktop development" workload.
2. XIVLauncher / Dalamud
   - The game must be launched via XIVLauncher with Dalamud enabled.
3. .NET 8 SDK
   - Required for building modern Dalamud plugins.

## Installation Steps

### 1. Create the Project Shell
1. Open a terminal (PowerShell or Command Prompt).
2. Install the Dalamud templates:
   dotnet new install GoatCorp.Dalamud.Templates
3. Create a new directory for your project and navigate into it.
4. Generate the plugin project:
   dotnet new dalamud -n ProjectYahu

### 2. Integrate Project Yahu Logic
1. Download the source files from this repository.
2. Copy the following folders from the repository into your new ProjectYahu directory:
   - Core
   - Models
   - Services
   - Common
3. Copy the following files into the root of your ProjectYahu directory, overwriting existing files if prompted:
   - Plugin.cs
   - ProjectYahu.csproj

### 3. Build the Plugin
1. Open ProjectYahu.csproj in Visual Studio 2022.
2. Right-click the project in Solution Explorer and select "Manage NuGet Packages".
3. Install the following packages if they are missing:
   - Newtonsoft.Json
   - DalamudPackager
4. Ensure your project configuration is set to "Debug" or "Release" and the platform is "x64".
5. Press F6 to build the solution.
6. Note the location of the output DLL (e.g., bin/x64/Debug/ProjectYahu.dll).

### 4. Load the Plugin in FFXIV
1. Launch Final Fantasy XIV.
2. Type /xlsettings in the chat window.
3. Navigate to the "Experimental" tab.
4. Under the "Dev Plugins" section, add the full path to your ProjectYahu.dll file.
5. Click "Save and Close".

## Usage Instructions

### Command
Type /yahu in the game chat to toggle the interface.

### Market Analysis Tab
1. This tab scans your current inventory.
2. Click "Start Analysis" to fetch real-time data from Universalis.
3. The results table will display:
   - Target Price: The calculated optimal price based on market distribution.
   - Strategy: Whether to match the lowest valid price or apply a minor undercut.
   - Stack Size: Recommended quantity per listing based on sales history.

### Cross-World Arbitrage Tab
1. Enter the name of your Data Center (e.g., Aether, Primal, Light).
2. Click "Scan DC".
3. The table will identify the cheapest world to buy the item and the most profitable world to sell it.

### Configuration Tab
1. Manage your "Sell List" here.
2. Items must be added by their Item ID to be included in the scan.
3. Set a "Min Price" for each item to ensure the plugin never recommends a price below your profit threshold.

## Technical Details

- Outlier Filtering: Uses Median Absolute Deviation (MAD) to ignore price manipulation and bait listings.
- Memory Reading: Utilizes FFXIVClientStructs for direct inventory access.
- Data Source: Real-time market data provided by the Universalis API.
