using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class LootManager {
	private static List<Loot> loots = new List<Loot>();

    private static EntityManager em;
    public static void SetEntityManager(EntityManager _em) {
        em = _em;
    }

    public static List<Loot> GetLoots() {
		return loots;
	}

	public static void RemoveLoot(Loot loot) {
		loots.Remove(loot);
        em.PushLootToPool(loot.gameObject);
        UnityEngine.Object.Destroy(loot);
    }
	
    public static GameObject CreateLoot(string classname, int count, Vector2 pos, float angle, Vector2 vel = new Vector2()) {
		if(!LevelManager.instance.isMapActive)
			return null;

        GameObject loot_obj = em.PullLootFromPool();
        if (loot_obj == null)
            return null;

        string script = GameDataManager.instance.RootData[classname]["ScriptClass"].Value<string>();
        if (string.IsNullOrEmpty(script))
            script = "Loot";
        Loot loot = (Loot)loot_obj.AddComponent(Type.GetType(script));
		loot.Initialize(classname, count);
        loot.DisallowPickup();

		loot_obj.transform.position = pos;
        Vector3 ang = loot_obj.transform.eulerAngles;
        ang.z = angle;
		loot_obj.transform.eulerAngles = ang;

        loot.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        loot.GetComponent<Rigidbody2D>().velocity += vel;

		loots.Add(loot);

        return loot_obj;
    }
}
