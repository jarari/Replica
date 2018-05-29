using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LootManager : MonoBehaviour {
    public static LootManager instance;
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public GameObject CreateLoot(string classname, int count, Vector2 pos, float angle, Vector2 vel = new Vector2()) {
        GameObject loot_obj = (GameObject)Instantiate(Resources.Load("Prefab/Loot"), pos, new Quaternion());
        string script = (string)GameDataManager.instance.GetData("Data", classname, "ScriptClass");
        if (script == null || script.Length == 0)
            script = "Loot";
        Loot loot = (Loot)loot_obj.AddComponent(Type.GetType(script));
        loot.GetComponent<Rigidbody2D>().velocity += vel;
        Vector3 ang = loot_obj.transform.eulerAngles;
        ang.z = angle;
        loot_obj.transform.eulerAngles = ang;
        loot.Initialize(classname, count);
        return loot_obj;
    }
}
