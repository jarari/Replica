using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LootManager : MonoBehaviour {
    public static LootManager instance;
	private List<Loot> loots = new List<Loot>();

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

	public List<Loot> GetLoots() {
		return loots;
	}
	public void RemoveLoot(Loot loot) {
		loots.Remove(loot);
	}
	
    public GameObject CreateLoot(string classname, int count, Vector2 pos, float angle, Vector2 vel = new Vector2()) {
		if(!LevelManager.instance.isMapActive)
			return null;

        GameObject loot_obj = (GameObject)Instantiate(Resources.Load("Prefab/Loot"), pos, new Quaternion());
        string script = GameDataManager.instance.RootData[classname]["ScriptClass"].Value<string>();
        if (script == null || script.Length == 0)
            script = "Loot";
        Loot loot = (Loot)loot_obj.AddComponent(Type.GetType(script));
        loot.GetComponent<Rigidbody2D>().velocity += vel;
        Vector3 ang = loot_obj.transform.eulerAngles;
        ang.z = angle;
        loot_obj.transform.eulerAngles = ang;
        loot.Initialize(classname, count);
		loots.Add(loot);

        return loot_obj;
    }
}
