using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerHUD {
    private static Character player;
    private static GlobalUIManager uimanager;
    private static float healthRatio = 0;

    public static void Initialize() {
        uimanager = GlobalUIManager.instance;
        player = CharacterManager.instance.GetPlayer();
    }

    public static void DrawUI() {
        if (!uimanager)
            return;
        healthRatio = player.GetCurrentStat(CharacterStats.Health) / player.GetMaxStat(CharacterStats.Health);
        uimanager.CreateImage("hp_frame", Helper.GetSprite("Sprites/ui/HPbarFrame", "HPbarFrame"), new Vector2(250, 1000));
        uimanager.RescaleUI("hp_frame", 2, 2);
        uimanager.CreateImage("hp_bar", Helper.GetSprite("Sprites/ui/HpBar", "HpBar"), new Vector2(250, 1000));
        uimanager.SetImageTypeFilled("hp_bar", Image.FillMethod.Horizontal, 0, healthRatio);
        uimanager.CreateImage("ammo_gb", Helper.GetSprite("Sprites/ui/bulletngrenade", "bulletngrenade"), new Vector2(135, 925));
        uimanager.RescaleUI("ammo_gb", 2, 2);
    }

    public static void UpdateHealth() {
        float ratio = player.GetCurrentStat(CharacterStats.Health) / player.GetMaxStat(CharacterStats.Health);
        uimanager.LerpFill("hp_bar", healthRatio, ratio, 0.3f);
        healthRatio = ratio;
    }

    public static void UpdateAmmo(ItemTypes type) {
    }
}
