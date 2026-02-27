using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ProjectYahu.Models;

namespace ProjectYahu.Services
{
    public class ConfigurationService
    {
        public PluginConfig Config { get; private set; } = new();
        private readonly string _configPath;

        public ConfigurationService(string? customPath = null)
        {
            // Default to a local path for now; in Dalamud, this would be the plugin's config folder.
            _configPath = customPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProjectYahu.config.json");
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    Config = JsonSerializer.Deserialize<PluginConfig>(json) ?? new PluginConfig();
                }
                else
                {
                    Config = new PluginConfig();
                    Save();
                }
            }
            catch
            {
                Config = new PluginConfig();
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] Error saving configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves configuration for a specific item, or null if not in the Sell List.
        /// </summary>
        public ItemConfig? GetItemConfig(uint itemId)
        {
            return Config.SellList.FirstOrDefault(i => i.ItemId == itemId);
        }

        public void AddToSellList(ItemConfig item)
        {
            if (Config.SellList.Any(i => i.ItemId == item.ItemId)) return;
            Config.SellList.Add(item);
            Save();
        }

        public void RemoveFromSellList(uint itemId)
        {
            Config.SellList.RemoveAll(i => i.ItemId == itemId);
            Save();
        }
    }
}
