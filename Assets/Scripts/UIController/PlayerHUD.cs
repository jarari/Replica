using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerHUD {
    private static Character player;
    private static GlobalUIManager uimanager;
    private static float healthRatio = 0;
    private static int trajLineCount = 50;
    private static float trajLineWidth = 10;

    public static void Initialize() {
        uimanager = GlobalUIManager.instance;
        player = CharacterManager.instance.GetPlayer();
    }

    public static void DrawUI() {
        if (!uimanager)
            return;
        healthRatio = player.GetCurrentStat(CharacterStats.Health) / player.GetMaxStat(CharacterStats.Health);
        uimanager.CreateImage("hp_frame", Helper.GetSprite("Sprites/ui", "HPbarFrame"), new Vector2(326, 970));
        uimanager.RescaleUI("hp_frame", 1, 1);
        uimanager.CreateImage("hp_bar", Helper.GetSprite("Sprites/ui", "HpBar"), new Vector2(326, 970));
        uimanager.SetImageTypeFilled("hp_bar", Image.FillMethod.Horizontal, 0, healthRatio);
        uimanager.CreateImage("ammo_gb", Helper.GetSprite("Sprites/ui", "bulletngrenade"), new Vector2(326, 970));
        uimanager.RescaleUI("ammo_gb", 1, 1);
        uimanager.CreateText("ammo_b", new Vector2(125, 940), "0", 50, 50);
        uimanager.CreateText("ammo_g", new Vector2(232, 940), "0", 50, 50);
        EventManager.RegisterEvent("HUD_UpdateHealth", new EventManager.OnCharacterHealthChanged(UpdateHealth));
    }

    public static void DrawGrenadeTrajectory(string pose, float chargeAmount) {
        if (!uimanager)
            return;

		JDictionary attackData = GameDataManager.instance.RootData[pose];

		Vector3 throwpos =
			player.transform.position +
			new Vector3(
				attackData["MuzzlePos"]["X"].Value<float>() * player.GetFacingDirection(),
				attackData["MuzzlePos"]["Y"].Value<float>()
				);

        float throwang = attackData["ThrowAngle"].Value<float>();
        throwang = 90 - (90 - throwang) * player.GetFacingDirection();

        List<Vector3> traj = Helper.GetTrajectoryPath(throwpos, throwang, player.GetCurrentStat(CharacterStats.GrenadeThrowPower) * chargeAmount, player, trajLineCount);

        Vector2 pos = player.transform.position;
        pos.x = Mathf.Round(pos.x * Helper.PixelsPerUnit) / Helper.PixelsPerUnit;
        pos.y = Mathf.Round(pos.y * Helper.PixelsPerUnit) / Helper.PixelsPerUnit;
        uimanager.grenadeUImf.transform.position = pos;

        List<Vector3> vertices = new List<Vector3>();
        Vector3 up = new Vector3();
        for (int i = 0; i < traj.Count - 1; i++) {
            Vector3 dtraj = traj[i + 1] - traj[i];
            up = Quaternion.AngleAxis(90, Vector3.forward) * dtraj.normalized;
            vertices.Add(traj[i] + up * trajLineWidth / 2f - player.transform.position);
            vertices.Add(traj[i] - up * trajLineWidth / 2f - player.transform.position);
        }
        vertices.Add(traj[traj.Count - 1] + up * trajLineWidth / 2f - player.transform.position);
        vertices.Add(traj[traj.Count - 1] - up * trajLineWidth / 2f - player.transform.position);

        Mesh mesh = uimanager.grenadeUImesh;
        mesh.Clear();
        mesh.vertices = vertices.ToArray();

        List<int> triangles = new List<int>();
        for(int i = 0; i < traj.Count - 1; i++) {
            triangles.Add(2 * i + 2);
            triangles.Add(2 * i + 1);
            triangles.Add(2 * i);

            triangles.Add(2 * i + 1);
            triangles.Add(2 * i + 2);
            triangles.Add(2 * i + 3);
        }
        mesh.triangles = triangles.ToArray();

        Vector3[] normals = new Vector3[vertices.Count];
        for(int i = 0; i < vertices.Count; i++) {
            normals[i] = -Vector3.forward;
        }
        mesh.normals = normals;
        
        List<Vector2> uvs = new List<Vector2>();
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(0, 0));
        for (int i = 1; i < traj.Count; i++) {
            uvs.Add(new Vector2(i / (traj.Count - 1), 1));
            uvs.Add(new Vector2(i / (traj.Count - 1), 0));
        }
        mesh.uv = uvs.ToArray();

        List<Color> colors = new List<Color>();
        float segSize = trajLineCount / 4f;
        float segMid = segSize / 2f;
        colors.Add(new Color(1, 1, 1, 1));
        colors.Add(new Color(1, 1, 1, 1));
        for (int i = 1; i < traj.Count; i++) {
            float segNum = Mathf.Floor(i / segSize);
            float alpha = 1 - segNum * 0.25f;
            if (Mathf.Abs(i - segNum * segSize - segMid) > segMid - segNum * segMid / 6f)
                alpha = 0;
            colors.Add(new Color(1, 1, 1, alpha));
            colors.Add(new Color(1, 1, 1, alpha));
        }
        mesh.colors = colors.ToArray();
    }

    public static void UpdateHealth(Character c, float changed) {
        if (c != player)
            return;
        float ratio = player.GetCurrentStat(CharacterStats.Health) / player.GetMaxStat(CharacterStats.Health);
        uimanager.LerpFill("hp_bar", healthRatio, ratio, 0.3f);
        healthRatio = ratio;
    }

    public static void UpdateAmmo(ItemTypes type, int count) {
        if (type == ItemTypes.Ammo)
            uimanager.ChangeText("ammo_b", count.ToString());
        else if(type == ItemTypes.Grenade)
            uimanager.ChangeText("ammo_g", count.ToString());
    }

    public static void ShowHUD() {
        uimanager.gameObject.SetActive(true);
    }

    public static void HideHUD() {
        uimanager.gameObject.SetActive(false);
    }
}
