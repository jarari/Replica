﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WeaponGunkata : Weapon {
    public override void OnAttack(string eventname) {
        base.OnAttack(eventname);
        if (eventname == "hg_forward" || eventname == "hg_down" || eventname == "hg_up") {
            SetMuzzlePos(new Vector2((float)GameDataManager.instance.GetData("Data", eventname, "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData("Data", eventname, "MuzzlePos", "Y")));
        }
    }

    public void FireBullet(string bulletclass, float ang) {
        BulletManager.instance.CreateBullet(bulletclass, GetMuzzlePos(), owner, this, 90 - ang * owner.GetFacingDirection(), GetEssentialStats());
    }

    public override void OnWeaponEvent(string eventname) {
        if (eventname == "hg_forward" || eventname == "hg_down" || eventname == "hg_up") {
            SetMuzzlePos(new Vector2((float)GameDataManager.instance.GetData("Data", eventname, "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData("Data", eventname, "MuzzlePos", "Y")));
            if(eventname == "hg_up")
                FireBullet("bullet_hg", 45);
            else
                FireBullet("bullet_hg", 90);
        }
        else if(eventname == "attack_basic_finish_fire") {
            FireBullet("bullet_gunkata_finish", 90);
        }
    }
}
