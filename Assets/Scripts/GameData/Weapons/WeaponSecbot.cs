using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WeaponSecbot : Weapon {
    public override void OnAttack(string eventname) {
        base.OnAttack(eventname);
        if (eventname == "attack_secbot") {
            SetMuzzlePos(new Vector2((float)GameDataManager.instance.GetData("Data", eventname, "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData("Data", eventname, "MuzzlePos", "Y")));
        }
    }

    public override void OnWeaponEvent(string eventname) {
        if (eventname == "attack_secbot_fire") {
            FireBullet("bullet_secbot", 90);
        }
    }
}