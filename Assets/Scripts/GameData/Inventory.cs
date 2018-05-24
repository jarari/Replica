using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventorySlot {
    public Item item;
    public int count;
    public InventorySlot(Item i, int c) {
        item = i;
        count = c;
    }
    public void SetCount(int c) {
        count = c;
    }
}

enum SpaceStatus {
    Empty,
    Stackable,
    Full
}
public static class Inventory {
    private static List<InventorySlot> inventory = new List<InventorySlot>();
    private static int inventoryCount = 9;
    private static SpaceStatus CheckSpace(string itemname) {
        InventorySlot[] temp = inventory.ToArray();
        for(int i = 0; i < temp.Length; i++) {
            InventorySlot slot = temp[i];
            if (slot.item.GetName() == itemname && slot.item.IsStackable()) {
                inventory[i].SetCount(inventory[i].count + 1);
                return SpaceStatus.Stackable;
            }
        }
        if (inventory.Count > inventoryCount) {
            return SpaceStatus.Full;
        }
        return SpaceStatus.Empty;
    }

    public static void AddItem(string itemname) {
        SpaceStatus status = CheckSpace(itemname);
        if (status == SpaceStatus.Empty) {
            inventory.Add(new InventorySlot(CachedItem.GetItemClass(itemname), 1));
        }
    }

    public static void UseItem(int i) {
        if(inventory.ElementAtOrDefault(i) != null && inventory[i].count > 0) {
            if(inventory[i].item.GetItemType() == ItemTypes.Consumable)
                inventory[i].SetCount(inventory[i].count - 1);
            inventory[i].item.Use();
            if(inventory[i].item.GetItemType() == ItemTypes.Weapon)
                PlayerHUD.UpdateAmmo(0);
            PlayerPauseUI.UpdateInventoryCell(i);
        }
    }

    public static void RemoveItem(int i) {
        if (inventory.ElementAtOrDefault(i) != null) {
            inventory.RemoveAt(i);
        }
    }

    public static List<InventorySlot> GetInventory() {
        return inventory;
    }

    public static void SetInventory(List<InventorySlot> invenslot) {
        inventory = invenslot;
    }
}
