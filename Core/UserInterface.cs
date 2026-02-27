using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using ProjectYahu.Core;
using ProjectYahu.Models;
using ProjectYahu.Services;

namespace ProjectYahu.Core
{
    public class UserInterface : Window, IDisposable
    {
        private readonly ConfigurationService _config;
        private readonly MarketOrchestrator _orchestrator;
        private readonly CrossWorldAnalysis _crossWorld;
        private List<ListingPlan> _currentPlans = new();
        private Dictionary<uint, CrossWorldAnalysis.CrossWorldResult> _crossWorldResults = new();
        private bool _isAnalyzing = false;
        private string _targetWorld = "Cactuar"; // Should be fetched from ClientState
        private string _targetDC = "Aether";

        public event Action? OnRequestAnalysis;

        public UserInterface(ConfigurationService config, MarketOrchestrator orchestrator, CrossWorldAnalysis crossWorld) 
            : base("Project Yahu - Quiet Wealth")
        {
            _config = config;
            _orchestrator = orchestrator;
            _crossWorld = crossWorld;
            
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(400, 300),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
        }

        public void Dispose()
        {
            // Dispose managed resources if any
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("YahuTabs"))
            {
                if (ImGui.BeginTabItem("Market Analysis"))
                {
                    DrawMarketAnalysis();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Cross-World Arbitrage"))
                {
                    DrawCrossWorld();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Configuration"))
                {
                    DrawConfiguration();
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
        }

        private void DrawMarketAnalysis()
        {
            ImGui.Text("Analyze your inventory and optimize listings.");
            
            // World Input (Mock for now, hook to ClientState later)
            ImGui.InputText("Current World", ref _targetWorld, 32);

            if (ImGui.Button("Start Analysis") && !_isAnalyzing)
            {
                _isAnalyzing = true;
                OnRequestAnalysis?.Invoke();
            }

            if (_isAnalyzing)
            {
                ImGui.Text("Analyzing market data... please wait.");
                ImGui.ProgressBar(0.5f, new Vector2(ImGui.GetContentRegionAvail().X, 20), "Fetching...");
            }
            else
            {
                ImGui.Separator();
                if (_currentPlans == null || _currentPlans.Count == 0)
                {
                    ImGui.TextDisabled("No analysis results yet.");
                }
                else
                {
                    if (ImGui.BeginTable("ResultsTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("Item");
                        ImGui.TableSetupColumn("Action");
                        ImGui.TableSetupColumn("Price");
                        ImGui.TableSetupColumn("Stack");
                        ImGui.TableSetupColumn("Status");
                        ImGui.TableHeadersRow();

                        foreach (var plan in _currentPlans)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text(plan.Item.Name);
                            ImGui.TableNextColumn();
                            ImGui.Text(plan.Strategy.ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text($"{plan.TargetPrice:N0} gil");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{plan.TotalStacks}x {plan.StackSize}");
                            ImGui.TableNextColumn();
                            if (plan.Status == PlanStatus.Ready)
                                ImGui.TextColored(new Vector4(0, 1, 0, 1), "READY");
                            else
                                ImGui.TextColored(new Vector4(1, 0, 0, 1), plan.Status.ToString());
                        }
                        ImGui.EndTable();
                    }
                }
            }
        }

        public void SetIsAnalyzing(bool isAnalyzing)
        {
            _isAnalyzing = isAnalyzing;
        }

        public void SetPlans(List<ListingPlan> plans)
        {
            _currentPlans = plans;
            _isAnalyzing = false;
        }

        private void DrawCrossWorld()
        {
            ImGui.Text("Find profit opportunities across the Data Center.");
            ImGui.InputText("Data Center", ref _targetDC, 32);

            if (ImGui.Button("Scan DC for Sell List"))
            {
                _ = ScanDCAsync();
            }

            ImGui.Separator();
            
            if (_crossWorldResults.Count > 0)
            {
                 if (ImGui.BeginTable("ArbitrageTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                 {
                     ImGui.TableSetupColumn("Item");
                     ImGui.TableSetupColumn("Buy (Low)");
                     ImGui.TableSetupColumn("Sell (High)");
                     ImGui.TableSetupColumn("Margin");
                     ImGui.TableHeadersRow();

                     foreach (var kvp in _crossWorldResults)
                     {
                         var result = kvp.Value;
                         ImGui.TableNextRow();
                         ImGui.TableNextColumn();
                         ImGui.Text($"Item #{kvp.Key}"); // Ideally resolve name
                         ImGui.TableNextColumn();
                         ImGui.Text($"{result.BestBuyWorld} @ {result.BestBuyPrice}");
                         ImGui.TableNextColumn();
                         ImGui.Text($"{result.BestSellWorld} @ {result.BestSellPrice}");
                         ImGui.TableNextColumn();
                         var color = result.ProfitMargin > 0.2 ? new Vector4(0,1,0,1) : new Vector4(1,1,1,1);
                         ImGui.TextColored(color, $"{result.ProfitMargin:P0}");
                     }
                     ImGui.EndTable();
                 }
            }
        }

        private void DrawConfiguration()
        {
            ImGui.Text("Manage your Sell List.");
            // Simple list of items
            foreach (var item in _config.Config.SellList)
            {
                if (ImGui.TreeNode($"Item {item.ItemId} ({item.ItemName})"))
                {
                    var minPrice = (int)item.MinPrice;
                    if (ImGui.InputInt("Min Price", ref minPrice)) item.MinPrice = minPrice;
                    
                    var margin = (float)item.TargetMargin;
                    if (ImGui.InputFloat("Target Margin", ref margin)) item.TargetMargin = margin;

                    ImGui.TreePop();
                }
            }
            
            if (ImGui.Button("Save Configuration"))
            {
                _config.Save();
            }
        }

        private async System.Threading.Tasks.Task RunAnalysisAsync()
        {
            // Triggered from UI thread, but runs async
            // In real app, get real inventory here via service
            // For now, assume orchestrator handles it or passes mock
            // Wait, Orchestrator needs INVENTORY.
            // Since we don't have direct access to GameInventoryService here easily without injecting it,
            // we will simulate an empty inventory or need to pass it in.
            
            // To fix this properly: UI should call a delegate or event on Plugin.cs to run logic.
            // For now, we just reset the flag.
            
            await System.Threading.Tasks.Task.Delay(1000); // Mock delay
            _isAnalyzing = false;
        }

        private async System.Threading.Tasks.Task ScanDCAsync()
        {
            _crossWorldResults.Clear();
            foreach(var item in _config.Config.SellList)
            {
                var result = await _crossWorld.AnalyzeDataCenter(item.ItemId, _targetDC);
                _crossWorldResults[item.ItemId] = result;
            }
        }

        public void SetPlans(List<ListingPlan> plans)
        {
            _currentPlans = plans;
            _isAnalyzing = false;
        }
    }
}
