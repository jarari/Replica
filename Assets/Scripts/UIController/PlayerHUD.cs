using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerHUD {
    private static Character player;
    private static GlobalUIManager uimanager;
    private static int barX = -40;
    private static int barY = -50;
    private static int barPadding = 16;
    private static int barWidth = 0;
    private static int barHeight = 5;
    private static int ammoPaddingY = 15;

    public static void Initialize() {
        uimanager = GlobalUIManager.instance;
        barWidth = -barPadding - barX - barX;
        player = CharacterManager.instance.GetPlayer();
        uimanager.CreateImage("icon_hp", Helper.GetSprite("Sprites/ui/icon_hp", "icon_hp"), 
            player.transform.position + new Vector3(barX, barY), 32, 32, player.transform.parent);
        uimanager.CreateLeftToRightBar("bar_hp", new Color(0, 1, 0),
            player.transform.position + new Vector3(barX + barPadding, barY), barWidth, barHeight, player.transform.parent);
        uimanager.CreateImage("icon_ammo", Helper.GetSprite("Sprites/ui/icon_ammo", "icon_ammo"),
            player.transform.position + new Vector3(barX, barY - ammoPaddingY), 32, 32, player.transform.parent);
        uimanager.CreateLeftToRightBar("bar_ammo", new Color(0.5f, 0.5f, 0.5f),
            player.transform.position + new Vector3(barX + barPadding, barY - ammoPaddingY), barWidth, barHeight, player.transform.parent);
    }

    public static void UpdateHealth(float ratio) {
        uimanager.ResizeUI("bar_hp", (int)(ratio * barWidth), barHeight);
    }

    public static void UpdateAmmo(float ratio) {
        uimanager.ResizeUI("bar_ammo", (int)(ratio * barWidth), barHeight);
    }
}
