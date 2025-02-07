using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PauseMenu : Menu {
    protected override void PopulateMenu() {
        uimanager.CreatePanel("mainmenu_panel", uimanager.GetStandardScreenSize() / 2, transform, 460, 640);
        GameObject btn_inventory = uimanager.CreateButton("mainmenu_btn_inventory", "ShowMenu", "InventoryMenu", Helper.GetSprite("Sprites/ui/pauseui/", "pauseui_button"), uimanager.GetStandardScreenSize() / 2 + new Vector2(0, 150), transform, 320, 80);
        uimanager.CreateText("mainmenu_txt_inventory", uimanager.GetStandardScreenSize() / 2 + new Vector2(0, 150), "Inventory", transform, 320, 80);
        uimanager.SetTextSize("mainmenu_txt_inventory", 50);
        uimanager.RescaleUI("mainmenu_txt_inventory", 1.2f, 1);
        GameObject btn_mainmenu = uimanager.CreateButton("mainmenu_btn_mainmenu", "MainMenu", "", Helper.GetSprite("Sprites/ui/pauseui/", "pauseui_button"), uimanager.GetStandardScreenSize() / 2 - new Vector2(0, 150), transform, 320, 80);
        uimanager.CreateText("mainmenu_txt_mainmenu", uimanager.GetStandardScreenSize() / 2 - new Vector2(0, 150), "Main Menu", transform, 320, 80);
        uimanager.SetTextSize("mainmenu_txt_mainmenu", 50);
        uimanager.RescaleUI("mainmenu_txt_mainmenu", 1.2f, 1);
        uimanager.FocusObject(btn_inventory);

        MenuManager.instance.pauseCount++;
        Cursor.visible = true;
        PlayerHUD.HideHUD();
    }

    public override void DestroyMenu() {
        base.DestroyMenu();
        MenuManager.instance.pauseCount--;
        Cursor.visible = false;
        PlayerHUD.ShowHUD();
    }
}
