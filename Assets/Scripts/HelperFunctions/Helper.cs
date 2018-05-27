using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageData {
    public Character attacker;
    public Character victim;
    public bool isCrit;
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

    public static Sprite GetSprite(string path, string name) {
        foreach (Sprite sprite in Resources.LoadAll(path, typeof(Sprite))) {
            if (sprite.name == name)
                return sprite;
        }
        return null;
    }

    public static bool CalcChance(float chance) {
        return Random.Range(0.00f, 1.00f) <= chance;
    }

    public static DamageData DamageCalc(Character attacker, Dictionary<WeaponStats, float> attackerdata, Character victim, bool ignorearmor = false) {
        return new DamageData(attacker, victim, 100, 0);
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

    public static bool IsInBox(Vector2 pos, Vector2 boxMin, Vector2 boxMax) {
        float xMin = Mathf.Min(boxMin.x, boxMax.x);
        float xMax = Mathf.Max(boxMin.x, boxMax.x);
        float yMin = Mathf.Min(boxMin.y, boxMax.y);
        float yMax = Mathf.Max(boxMin.y, boxMax.y);
        if (pos.x >= xMin && pos.x <= xMax && pos.y >= yMin && pos.y <= yMax)
            return true;
        return false;
    }
}
