using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InventoryMenu : Menu {
    private GameObject scrollview;
    private int itemSize = 40;
    private int itemPadding = 10;
    protected override void PopulateMenu() {
        uimanager.CreatePanel("inventory_panel_item", new Vector2(305, 415), transform, 550, 800);
        scrollview = uimanager.CreateScrollView("inventory_scrollview_item", new Vector2(305, 400), transform, 550, 750);
        CreateItemList();
    }

    public void CreateItemList() {
        if(scrollview != null) {
            Transform content = scrollview.transform.GetChild(0).GetChild(0);
            Inventory inventory = CharacterManager.instance.GetPlayer().GetInventory();
            Sprite sprite_btn = Helper.GetSprite("Sprites/ui/pauseui/", "pauseui_button");
            int itemY = -40;
            foreach (InventorySlot slot in inventory.GetList()) {
                string classname = slot.item.GetClassName();
                GameObject btn_item = uimanager.CreateButton("inventory_" + classname, "UseItem", classname, sprite_btn, new Vector2(305, itemY), 520, itemSize);
                uimanager.CreateText("inventory_" + classname + "_name", new Vector2(265, itemY), slot.item.GetName(), btn_item.transform, 400, itemSize);
                uimanager.SetTextSize("inventory_" + classname + "_name", itemSize - 5);
                uimanager.RescaleUI("inventory_" + classname + "_name", 1.2f, 1);
                uimanager.CreateText("inventory_" + classname + "_count", new Vector2(480, itemY), slot.count.ToString(), btn_item.transform, 60, itemSize);
                uimanager.SetTextSize("inventory_" + classname + "_count", itemSize - 5);
                btn_item.transform.SetParent(content);
                itemY -= itemPadding + itemSize;
            }
        }
    }
}
