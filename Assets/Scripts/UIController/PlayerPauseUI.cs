using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PlayerPauseUI {
    private static bool paused = false;
    private static GameObject pauseUI = null;
    private static GameObject additionalUI = null;
    private static float zoomMenuAdjustX = 128;
    private static float zoomMenuAdjustY = 64;
    private static float YFromTitle = 34;
    private static float buttonPadding = 48;
    private static float inventoryCellY = YFromTitle + 85;
    private static int cellSize = 50;
    private static float cellPadding = 3;
    private static int lastMenu;
    private static int currentMenu;
    private static Dictionary<int, string> lastFocus = new Dictionary<int, string>();
    private static Vector3 menuPos;
    public static void PauseToggle() {
        paused = !paused;
        if (paused)
            TimeManager.instance.Pause();
        HandleUI();
    }

    public static bool IsPaused() {
        return paused;
    }

    private static void HandleUI() {
        if (paused) {
            CreateMainMenu();
        }
        else {
            if (pauseUI != null)
                Object.Destroy(pauseUI);
            if (additionalUI != null)
                Object.Destroy(additionalUI);
            CamController.instance.ZoomCam(CharacterManager.instance.GetPlayer().transform.position + new Vector3(0, CamController.instance.GetCamUp(), 0), 1f, 0.5f);
        }
    }

    private static void CreateMainMenu() {
        GlobalUIManager uimanager = GlobalUIManager.instance;
        menuPos = CharacterManager.instance.GetPlayer().transform.position + new Vector3(zoomMenuAdjustX, zoomMenuAdjustY, 0);
        CamController.instance.ZoomCam(CharacterManager.instance.GetPlayer().transform.position + new Vector3(zoomMenuAdjustX - 32f, zoomMenuAdjustY, 0), 2f, 0.5f);
        pauseUI = uimanager.CreateImage("pauseui_bg", Helper.GetSprite("Sprites/ui/pauseui/pauseui_bg", "pauseui_bg"), menuPos, 234, 208);
        Vector3 btnPos = menuPos + new Vector3(14, 104 - YFromTitle - buttonPadding, 0);
        uimanager.FocusObject(uimanager.CreateButton("pauseui_btn_stats", "ShowMenu", 1, Helper.GetSprite("Sprites/ui/pauseui/pauseui_button_stats", "pauseui_button_stats"), btnPos, 181, 38, pauseUI.transform));
        btnPos -= new Vector3(0, buttonPadding, 0);
        uimanager.CreateButton("pauseui_btn_inventory", "ShowMenu", 2, Helper.GetSprite("Sprites/ui/pauseui/pauseui_button_inventory", "pauseui_button_inventory"), btnPos, 181, 38, pauseUI.transform);
        btnPos -= new Vector3(0, buttonPadding, 0);
        uimanager.CreateButton("pauseui_btn_option", "ShowMenu", 3, Helper.GetSprite("Sprites/ui/pauseui/pauseui_button_option", "pauseui_button_option"), btnPos, 181, 38, pauseUI.transform);
        lastMenu = 0;
        currentMenu = 0;
        if (lastFocus.ContainsKey(currentMenu) && lastFocus[currentMenu] != null) {
            uimanager.FocusObject(uimanager.GetUIObject(lastFocus[currentMenu]));
        }
    }

    private static void CreateInventory() {
        List<InventorySlot> inv = CharacterManager.instance.GetPlayer().GetInventory().GetList();
        GlobalUIManager uimanager = GlobalUIManager.instance;
        additionalUI = uimanager.CreateImage("inventory_bg", Helper.GetSprite("Sprites/ui/pauseui/inventory_bg", "inventory_bg"), menuPos, 234, 208);
        Vector3 cellsPos = menuPos + new Vector3(14, 104 - inventoryCellY, 0);
        uimanager.CreateImage("inventory_cells", Helper.GetSprite("Sprites/ui/pauseui/inventory_cells", "inventory_cells"), cellsPos, 170, 170, additionalUI.transform);
        for(int i = 1; i > -2; i--) { //row ↓
            for(int j = -1; j < 2; j++) { //column ->
                int buttonnum = (1 - i) * 3 + (j + 1);
                GameObject button = uimanager.CreateButton("inventory_cell" + buttonnum.ToString(), "item_showmenu", buttonnum,
                    Helper.GetSprite("Sprites/ui/pauseui/inventory_button", "inventory_button"), cellsPos
                    + new Vector3(j * (cellSize + cellPadding), i * (cellSize + cellPadding), 0), cellSize, cellSize, additionalUI.transform);
                Color c = button.GetComponent<Button>().colors.normalColor;
                c.a = 0;
                ColorBlock cb = button.GetComponent<Button>().colors;
                cb.normalColor = c;
                button.GetComponent<Button>().colors = cb;
                button.name = "inventory_cell" + buttonnum.ToString();
                if(inv.ElementAtOrDefault(buttonnum) != null) {
                    uimanager.CreateImage("inventory_image" + buttonnum.ToString(), inv[buttonnum].item.GetImage(), cellsPos
                    + new Vector3(j * (cellSize + cellPadding), i * (cellSize + cellPadding), 0), cellSize, cellSize, button.transform);
                }
                if (i == 1 && j == -1)
                    uimanager.FocusObject(button);
            }
        }
        if (lastFocus.ContainsKey(currentMenu) && lastFocus[currentMenu] != null) {
            uimanager.FocusObject(uimanager.GetUIObject(lastFocus[currentMenu]));
        }
    }

    public static void UpdateInventoryCell(int i) {
        Inventory inventory = CharacterManager.instance.GetPlayer().GetInventory();
        List<InventorySlot> slots = inventory.GetList();
        if (slots.ElementAtOrDefault(i) == null) return;
        if (slots.ElementAtOrDefault(i).count == 0) {
            inventory.RemoveItem(i);
            Object.Destroy(GlobalUIManager.instance.GetUIObject("inventory_image" + i.ToString()));
        }
        //TODO : update item count
    }

    public static void ShowMenu(int menu) {
        GlobalUIManager uimanager = GlobalUIManager.instance;
        if (!lastFocus.ContainsKey(currentMenu))
            lastFocus.Add(currentMenu, uimanager.GetNameFromUIObject(uimanager.GetCurrentFocus()));
        else
            lastFocus[currentMenu] = uimanager.GetNameFromUIObject(uimanager.GetCurrentFocus());
        if (menu != 0) {
            Object.Destroy(pauseUI);
        }
        currentMenu = menu;
        switch (menu) {
            case 0: //Main
                if (additionalUI != null)
                    Object.Destroy(additionalUI);
                CreateMainMenu();
                break;
            case 1: //Stats
                lastMenu = 0;
                break;
            case 2: //Inventory
                lastMenu = 0;
                CreateInventory();
                break;
            case 3: //Option
                lastMenu = 0;
                break;
        }
    }

    public static void GoBack() {
        if (currentMenu != 0)
            ShowMenu(lastMenu);
        else
            PauseToggle();
    }
}
