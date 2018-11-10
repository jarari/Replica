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
    public void SlotSetCount(int c) {
        count = c;
    }
}

enum SpaceStatus {
    Empty,
    Stackable,
    Full
}
public class Inventory : MonoBehaviour {
    private List<InventorySlot> inventory = new List<InventorySlot>();
    private int inventoryCount = 9;
    private GameObject owner;

    private int GetItemIndex(string itemname) {
        InventorySlot[] temp = inventory.ToArray();
        for (int i = 0; i < temp.Length; i++) {
            InventorySlot slot = temp[i];
            if (slot.item.GetClassName() == itemname) {
                return i;
            }
        }
        return -1;
    }
    private SpaceStatus CheckSpace(string itemname) {
        InventorySlot[] temp = inventory.ToArray();
        for(int i = 0; i < temp.Length; i++) {
            InventorySlot slot = temp[i];
            if (slot.item.GetClassName() == itemname && slot.item.IsStackable()) {
                return SpaceStatus.Stackable;
            }
        }
        if (inventory.Count > inventoryCount) {
            return SpaceStatus.Full;
        }
        return SpaceStatus.Empty;
    }

    public GameObject GetOwner() {
        return owner;
    }

    public void SetOwner(GameObject c) {
        owner = c;
    }

    public void AddItem(string itemname, int count = 1) {
        SpaceStatus status = CheckSpace(itemname);
        if (status == SpaceStatus.Empty) {
            inventory.Add(new InventorySlot(CachedItem.GetItemClass(itemname), count));
            OnContentChange(CachedItem.GetItemClass(itemname), inventory.Count - 1, 0, count);
        }
        else if(status == SpaceStatus.Stackable) {
            int i = GetItemIndex(itemname);
            OnContentChange(inventory[i].item, i, inventory[i].count, inventory[i].count + count);
            inventory[i].SlotSetCount(inventory[i].count + count);
        }
    }

    public void UseItem(int i, int count) {
        if (inventory.ElementAtOrDefault(i) != null && inventory[i].count > 0) {
            if (inventory[i].item.GetItemType() != ItemTypes.InfiniteUse
                || inventory[i].item.GetItemType() != ItemTypes.Weapon)
                inventory[i].SlotSetCount(inventory[i].count - count);
            inventory[i].item.Use();

			EventManager.OnItemUsed(owner, inventory[i].item, count);
		}
    }

    public int GetCount(string itemname) {
        int i = GetItemIndex(itemname);
        if (i == -1)
            return 0;
        return inventory[i].count;
    }

    public void SetCount(int i, int count) {
        if (inventory.ElementAtOrDefault(i) != null && inventory[i].count > 0) {
            OnContentChange(inventory[i].item, i, inventory[i].count, count);
            inventory[i].SlotSetCount(count);
        }
    }

    public void SetCount(string itemname, int count) {
        int i = GetItemIndex(itemname);
        if (i != -1)
            SetCount(i, count);
    }

    public void ModCount(int i, int count) {
        if (inventory.ElementAtOrDefault(i) != null && inventory[i].count > 0) {
            OnContentChange(inventory[i].item, i, inventory[i].count, inventory[i].count + count);
            inventory[i].SlotSetCount(inventory[i].count + count);
        }
    }

    public void ModCount(string itemname, int count) {
        int i = GetItemIndex(itemname);
        if (i != -1)
            ModCount(i, count);
    }

    public void UseItem(string itemname, int count) {
        int i = GetItemIndex(itemname);
        if (i != -1)
            UseItem(i, count);
    }

    public void RemoveItem(int i) {
        if (inventory.ElementAtOrDefault(i) != null) {
            OnContentChange(inventory[i].item, i, inventory[i].count, 0);
            inventory.RemoveAt(i);
        }
    }

    public void RemoveItem(string itemname) {
        int i = GetItemIndex(itemname);
        if (i != -1)
            RemoveItem(i);
    }

    public List<InventorySlot> GetList() {
        return inventory;
    }

    public void SetInventory(List<InventorySlot> i) {
        inventory = i;
    }

    public void EmptyInventory() {
        inventory.Clear();
    }

    public void OnContentChange(Item changed, int slot, int oldcount, int newcount) {
        int deltacount = newcount - oldcount;

        if(deltacount > 0)
			EventManager.OnItemAdded(owner, this, changed, deltacount);
        
        else if(deltacount < 0)
			EventManager.OnItemRemoved(owner, this, changed, deltacount);
        
        if (owner.tag.Equals("Character")) {
            Character c = owner.GetComponent<Character>();
            if(c != null) {
                if(c.IsPlayer())
                    if (changed.GetItemType() == ItemTypes.Ammo || changed.GetItemType() == ItemTypes.Grenade) {
                        PlayerHUD.UpdateAmmo(changed.GetItemType(), newcount);
                        if (changed.GetItemType() == ItemTypes.Ammo)
                            c.GetAnimator().SetInteger("Ammo", newcount);
                    }
            }
        }
    }
}
