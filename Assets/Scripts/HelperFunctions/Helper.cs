﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageData {
    public Character attacker;
    public Character victim;
    public float damage;
    public float stagger;
    public DamageData(Character a, Character v, float dmg, float stag) {
        attacker = a;
        victim = v;
        damage = dmg;
        stagger = stag;
    }
}

public static class Helper {
    private static float indicator_rand = 30f;
    private static float indicator_yvel = 30f;
    private static float indicator_yup = 30f;
    public static int PixelsPerUnit = 1;
    public static LayerMask characterLayer;
    public static LayerMask mapLayer;
    public static LayerMask groundLayer;
    public static LayerMask ceilingLayer;

    public static void InitializeLayerMasks() {
        characterLayer = (1 << LayerMask.NameToLayer("Characters"));
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        mapLayer = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Ceiling"));
        ceilingLayer = 1 << LayerMask.NameToLayer("Ceiling");
    }

    public static Sprite GetSprite(string path, string name) {
        foreach (Sprite sprite in Resources.LoadAll(path, typeof(Sprite))) {
            if (sprite.name == name)
                return sprite;
        }
        return null;
    }

    public static PhysicsMaterial2D GetPhysicsMaterial2D(string name) {
        foreach (PhysicsMaterial2D mat in Resources.LoadAll("Sprites/tileset/physicsmat", typeof(PhysicsMaterial2D))) {
            if (mat.name == name)
                return mat;
        }
        return null;
    }

    public static bool CalcChance(float chance) {
        return Random.Range(0.00f, 1.00f) <= chance;
    }

    public static DamageData DamageCalc(Character attacker, Dictionary<WeaponStats, float> stats, Character victim, bool isRanged = false, bool ignorearmor = false) {
        float damage = stats[WeaponStats.Damage];
        float stagger = stats[WeaponStats.Stagger];
        if (!ignorearmor) {
            if (victim.GetCurrentStat(CharacterStats.SuperArmor) >= 0) {
                stagger = 0;
                victim.ModStat(CharacterStats.SuperArmor, -stats[WeaponStats.SADestruction]);
                if (victim.GetCurrentStat(CharacterStats.SuperArmor) == 0) {
                    EffectManager.instance.CreateEffect("effect_indicator_armorpen", victim.transform.position, 0);
                }
            }
            float armor = victim.GetCurrentStat(CharacterStats.MeleeArmor);
            if (isRanged)
                armor = victim.GetCurrentStat(CharacterStats.RangeArmor);
            damage -= -damage * (armor / (armor + 100f));
        }
        return new DamageData(attacker, victim, damage, stagger);
    }

    public static float BallisticsAngCalc(float distance, float height, float velocity, bool min) {
        float partial = Mathf.Sqrt(Mathf.Pow(velocity, 4) - 981 * (981 * Mathf.Pow(distance, 2) + 2 * height * Mathf.Pow(velocity, 2)));
        float ang = 0;
        if (min)
            ang = Mathf.Min(Mathf.Atan((Mathf.Pow(velocity, 2) + partial) / (981 * distance)), Mathf.Atan((Mathf.Pow(velocity, 2) - partial) / (981 * distance)));
        else
            ang = Mathf.Max(Mathf.Atan((Mathf.Pow(velocity, 2) + partial) / (981 * distance)), Mathf.Atan((Mathf.Pow(velocity, 2) - partial) / (981 * distance)));
        ang *= Mathf.Rad2Deg;
        return ang;
    }

    public static float BallisticsMaxRangeCalc(float velocity) {
        return Mathf.Pow(velocity, 2) / 981f;
    }

    public static float Vector2ToAng(Vector2 vec) {
        float ang = 0;
        if (vec.y >= 0)
            ang = Mathf.Acos(vec.normalized.x);
        else
            ang = Mathf.PI * 2 - Mathf.Acos(vec.normalized.x);
        ang *= Mathf.Rad2Deg;
        return ang;
    }

    private static bool overlap1D(float minX, float maxX, float minX2, float maxX2) {
        if (maxX >= minX2 && maxX2 >= minX)
            return true;
        return false;
    }

    public static bool IsInBox(Vector2 boxMin, Vector2 boxMax, Vector2 box2Min, Vector2 box2Max) {
        float xMin = Mathf.Min(boxMin.x, boxMax.x);
        float xMax = Mathf.Max(boxMin.x, boxMax.x);
        float yMin = Mathf.Min(boxMin.y, boxMax.y);
        float yMax = Mathf.Max(boxMin.y, boxMax.y);
        float xMin2 = Mathf.Min(box2Min.x, box2Max.x);
        float xMax2 = Mathf.Max(box2Min.x, box2Max.x);
        float yMin2 = Mathf.Min(box2Min.y, box2Max.y);
        float yMax2 = Mathf.Max(box2Min.y, box2Max.y);
        if (overlap1D(xMin, xMax, xMin2, xMax2) && overlap1D(yMin, yMax, yMin2, yMax2))
            return true;
        return false;
    }

    public static Vector2 SnapToBox(Vector2 pos, Vector2 boxOffset, Vector2 boxSize, Vector2 from) {
        Vector2 boxMin = pos + boxOffset - boxSize / 2f;
        Vector2 boxMax = pos + boxOffset + boxSize / 2f;
        return new Vector2(Mathf.Clamp(from.x, boxMin.x, boxMax.x), Mathf.Clamp(from.y, boxMin.y, boxMax.y));
    }

    public static Vector2 SnapToBox(Vector2 pos, BoxCollider2D box, Vector2 from) {
        return SnapToBox(pos, box.offset, box.size, from);
    }

    public static Vector2 GetClosestBoxBorder(Vector2 pos, Vector2 boxOffset, Vector2 boxSize, Vector2 from) {
        Vector2 boxMin = pos + boxOffset - boxSize / 2f;
        Vector2 boxMax = pos + boxOffset + boxSize / 2f;
        Vector2 result = new Vector2(Mathf.Clamp(from.x, boxMin.x, boxMax.x), Mathf.Clamp(from.y, boxMin.y, boxMax.y));
        if (result == from) {
            Vector2 boxCenter = (boxMin + boxMax) / 2f;
            result = GetClosestBoxBorder(pos, boxOffset, boxSize, (from - boxCenter).normalized * Mathf.Max(boxSize.x, boxSize.y));
        }
        return result;
    }

    public static Vector2 GetClosestBoxBorder(Vector2 pos, BoxCollider2D box, Vector2 from) {
        return GetClosestBoxBorder(pos, box.offset, box.size, from);
    }

    public static bool IsBlockedByMap(Vector2 pos, Vector2 another) {
        return Physics2D.Raycast(pos, another - pos, Vector3.Distance(pos, another), Helper.mapLayer).collider != null;
    }

    public static IEnumerator DrawBox(Vector2 pos, Vector2 area, Color col, float dur = 0.5f) {
        float elapsed = 0;
        float duration = dur;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            float minX = pos.x - area.x / 2f;
            float maxX = pos.x + area.x / 2f;
            float minY = pos.y - area.y / 2f;
            float maxY = pos.y + area.y / 2f;
            Debug.DrawLine(new Vector2(minX, minY), new Vector2(minX, maxY), col);
            Debug.DrawLine(new Vector2(minX, maxY), new Vector2(maxX, maxY), col);
            Debug.DrawLine(new Vector2(maxX, minY), new Vector2(maxX, maxY), col);
            Debug.DrawLine(new Vector2(minX, minY), new Vector2(maxX, minY), col);

            yield return null;
        }
    }

    public static IEnumerator DrawLine(Vector2 pos, Vector2 another, Color col, float dur = 0.5f) {
        float elapsed = 0;
        float duration = dur;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            Debug.DrawLine(pos, another, col);

            yield return null;
        }
    }
}
