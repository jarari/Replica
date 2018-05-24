using System;
using UnityEngine;

public enum WeaponStats {
    Damage,
    SADestruction,
    Spread,
    AttackSpeed,
    Stagger,
    MagSize,
    ReloadTime,
    Range,
    EndOfEnums
}

public enum WeaponTypes {
    Fist,
    Pistol,
    Sword
}

public abstract class Weapon : ObjectBase {

    WeaponTypes type;
    Character owner;
    public Vector2 muzzlePos;

    public override void Initialize(string classname) {
        base.Initialize(classname);
        if (owner) {
            for (int i = 0; i < (int)WeaponStats.EndOfEnums; i++) {
                owner.SetBaseStat((WeaponStats)i, owner.GetBaseStat((WeaponStats)i));
            }
            GetComponent<SpriteRenderer>().sortingOrder = GetOwner().GetComponent<SpriteRenderer>().sortingOrder + 1;
        }
        type = (WeaponTypes)Convert.ToInt32(GameDataManager.instance.GetData("Data", classname, "Stats", "WeaponType"));
    }

    public Character GetOwner() {
        return owner;
    }

    public void SetOwner(Character c) {
        owner = c;
    }

    public abstract void OnAttack();
}