using System.Collections;
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
    private static Dictionary<string, Material> cachedMaterials = new Dictionary<string, Material>();
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

    public static PhysicsMaterial2D GetPhysicsMaterial2D(string path, string name) {
        foreach (PhysicsMaterial2D mat in Resources.LoadAll(path, typeof(PhysicsMaterial2D))) {
            if (mat.name == name)
                return mat;
        }
        return null;
    }

    public static Material GetMaterial(string path, string name) {
        foreach (Material mat in Resources.LoadAll(path, typeof(Material))) {
            if (mat.name == name)
                return mat;
        }
        return null;
    }

    public static Texture GetTexture(string path, string name) {
        foreach (Texture tex in Resources.LoadAll(path, typeof(Texture))) {
            if (tex.name == name)
                return tex;
        }
        return null;
    }

    public static Flare GetFlare(string path, string name) {
        foreach (Flare flr in Resources.LoadAll(path, typeof(Flare))) {
            if (flr.name == name)
                return flr;
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
        float absGrav = Mathf.Abs(Physics2D.gravity.y);
        float partial = Mathf.Sqrt(Mathf.Pow(velocity, 4) - absGrav * (absGrav * Mathf.Pow(distance, 2) + 2 * height * Mathf.Pow(velocity, 2)));
        float ang = 0;
        if (min)
            ang = Mathf.Min(Mathf.Atan((Mathf.Pow(velocity, 2) + partial) / (absGrav * distance)), Mathf.Atan((Mathf.Pow(velocity, 2) - partial) / (absGrav * distance)));
        else
            ang = Mathf.Max(Mathf.Atan((Mathf.Pow(velocity, 2) + partial) / (absGrav * distance)), Mathf.Atan((Mathf.Pow(velocity, 2) - partial) / (absGrav * distance)));
        ang *= Mathf.Rad2Deg;
        return ang;
    }

    private static List<Vector3> polyFit(List<Vector3> poly, int lineCount) {
        List<Vector3> fitted = new List<Vector3>();
        float b = 0;
        float c = -Mathf.Infinity;
        float xmin = Mathf.Infinity;
        float xmax = -Mathf.Infinity;
        foreach(Vector3 point in poly) {
            if(c < point.y) {
                c = point.y;
                b = point.x;
            }
            xmin = Mathf.Min(xmin, point.x);
            xmax = Mathf.Max(xmax, point.x);
        }
        //a(x-b)^2 + c
        //a(point.x - b)^2 + c = point.y
        //a(point.x - b)^2 = point.y - c
        //a = (point.y - c) / (point.x - b)^2
        float xminusb = poly[0].x - b;
        int index = 0;
        if (xminusb == 0) {
            xminusb = poly[poly.Count - 1].x - b;
            index = poly.Count - 1;
        }
        float a = (poly[index].y - c) / Mathf.Pow(xminusb, 2);
        float dx = (xmax - xmin) / (lineCount - 1);
        for(int i = 0; i < lineCount; i++) {
            fitted.Add(new Vector2(xmin + dx * i, a * Mathf.Pow(xmin + dx * i - b, 2) + c));
        }
        return fitted;
    }

    public static List<Vector3> GetTrajectoryPath(Vector3 origin, float angle, float velocity, Character filter, int lineCount = 20) {
        List<Vector3> traj = new List<Vector3>();
        Vector3 pos = origin;
        pos.z = origin.z + 10;
        Vector3 vel = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)) * velocity;
        Vector3 grav = Physics2D.gravity;
        for(int i = 0; i < lineCount; i++) {
            traj.Add(pos);
            vel = vel + grav * Time.fixedDeltaTime;
            RaycastHit2D rayhit = Physics2D.Raycast(pos, pos + vel * Time.fixedDeltaTime, (vel * Time.fixedDeltaTime).magnitude, mapLayer | characterLayer);
            if (rayhit.collider != null) {
                if (rayhit.collider.CompareTag("Character")) {
                    if(rayhit.collider.GetComponent<Character>() != filter) {
                        pos = rayhit.point;
                        break;
                    }
                    else {
                        pos = pos + vel * Time.fixedDeltaTime;
                    }
                }
                else {
                    pos = rayhit.point;
                    break;
                }
            }
            else {
                pos = pos + vel * Time.fixedDeltaTime;
            }
        }
        pos.z = origin.z + 10;
        traj.Add(pos);
        traj = polyFit(traj, lineCount);
        return traj;
    }

    public static float BallisticsMaxRangeCalc(float velocity) {
        return Mathf.Pow(velocity, 2) / Mathf.Abs(Physics2D.gravity.y);
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
