using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class LootData {
    public int numMin = 0;
    public int numMax = 0;
    public float chance = 0;
    public string item = "";
    public LootData(string i, float c, int nmin, int nmax) {
        item = i;
        numMin = nmin;
        numMax = nmax;
        chance = c;
    }
}

public class Loot : ObjectBase {
    protected bool canPickup = false;
    protected string item = "";
    protected int count = 0;
    public virtual void Initialize(string classname, int _count) {
        base.Initialize(classname);
        if(GameDataManager.instance.GetData("Data", classname, "Item") != null)
            item = (string)GameDataManager.instance.GetData("Data", classname, "Item");
        count = _count;
    }
    public void AllowPickup() {
        gameObject.layer = LayerMask.NameToLayer("Bullet");
        box.isTrigger = true;
        rb.bodyType = RigidbodyType2D.Static;
        canPickup = true;
    }

    public void DisallowPickup() {
        gameObject.layer = LayerMask.NameToLayer("Projectile");
        box.isTrigger = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        canPickup = false;
    }

    protected void LandDust() {
        if (GameDataManager.instance.GetData("Data", className, "Sprites", "dust") != null)
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "dust"),
                transform.position, 0);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision) {
        if (!canPickup) return;
        Character c = null;
        if (collision.gameObject.tag.Equals("Character")) {
            c = collision.gameObject.GetComponent<Character>();
            if (c.IsPlayer()) {
                Pickup(c);
            }
            else {
                Physics2D.IgnoreCollision(box, collision);
            }
        }
    }

    public virtual void Pickup(Character c) {
        if (item.Length != 0) {
            c.GetInventory().AddItem(item, count);
        }
        if (GameDataManager.instance.GetData("Data", className, "Sprites", "pickup") != null)
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "pickup"),
                transform.position, 0);
        if (GameDataManager.instance.GetData("Data", className, "Sprites", "particle") != null)
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "pickup"),
                c.transform.position, 0, c.transform);
        DestroyObject(gameObject);
    }

	public void OnDestroy() {
		LootManager.instance.RemoveLoot(this);
	}
}
