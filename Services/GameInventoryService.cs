using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using ProjectYahu.Core;
using Lumina.Excel.GeneratedSheets;

namespace ProjectYahu.Services
{
    public unsafe class GameInventoryService
    {
        private readonly IDataManager _dataManager;

        public GameInventoryService(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        /// <summary>
        /// Reads all items from the player's main Inventory.
        /// </summary>
        public List<InventoryItem> GetAllInventoryItems()
        {
            var results = new List<InventoryItem>();
            var manager = InventoryManager.Instance();
            var itemSheet = _dataManager.GetExcelSheet<Item>();

            if (manager == null || itemSheet == null) return results;

            // Simple iteration through standard inventory (Page 1-4)
            // InventoryType is usually 0, 1, 2, 3 for Bag 1-4
            // Or enum values: Inventory1 = 0, Inventory2 = 1, Inventory3 = 2, Inventory4 = 3
            
            for (int bagIndex = 0; bagIndex < 4; bagIndex++)
            {
                var container = manager->GetInventoryContainer((InventoryType)bagIndex);
                if (container == null) continue;

                for (int slotIndex = 0; slotIndex < container->Size; slotIndex++)
                {
                    var slot = container->GetInventorySlot(slotIndex);
                    // Skip empty slots or huge quantity bugs (sometimes happens with bad pointers)
                    if (slot == null || slot->ItemID == 0 || slot->Quantity == 0) continue;

                    var itemRow = itemSheet.GetRow(slot->ItemID);
                    string itemName = itemRow?.Name.ToString() ?? $"Item #{slot->ItemID}";

                    results.Add(new InventoryItem
                    {
                        ItemId = slot->ItemID,
                        Name = itemName,
                        Count = (int)slot->Quantity,
                        IsHq = (slot->Flags & InventoryItem.ItemFlags.HQ) != 0,
                        Slot = slotIndex
                    });
                }
            }

            return results;
        }
    }
}
