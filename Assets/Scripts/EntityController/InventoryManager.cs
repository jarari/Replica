using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class InventoryManager {
    static Dictionary<GameObject, Inventory> inventories = new Dictionary<GameObject, Inventory>();

    public static Dictionary<GameObject, Inventory> GetFullList() {
        return inventories;
    }

    public static GameObject GetOwner(Inventory inventory) {
        foreach(KeyValuePair<GameObject, Inventory> kvp in inventories) {
            if (kvp.Value == inventory)
                return kvp.Key;
        }
        return null;
    }

    public static void CreateInventory(GameObject obj) {
		if(!LevelManager.instance.isMapActive) return;
		if (obj.GetComponent<ObjectBase>() == null)
            return;
        Inventory inventory = obj.AddComponent<Inventory>();
        inventory.SetOwner(obj);
        inventories.Add(obj, inventory);
        obj.GetComponent<ObjectBase>().SetInventory(inventory);
    }
}
