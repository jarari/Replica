using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 건카타/권총 무기 클래스 */
public class WeaponGunkata : Weapon {
    public override void OnAttack(string eventname) {
        base.OnAttack(eventname);
        if (eventname == "hg_forward" || eventname == "hg_down" || eventname == "hg_up") {
            SetMuzzlePos(new Vector2((float)GameDataManager.instance.GetData(eventname, "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData(eventname, "MuzzlePos", "Y")));
        }
    }

    public override void OnWeaponEvent(string eventname) {
        base.OnWeaponEvent(eventname);
        if (eventname == "hg_forward" || eventname == "hg_down" || eventname == "hg_up") {
            SetMuzzlePos(new Vector2((float)GameDataManager.instance.GetData(eventname, "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData(eventname, "MuzzlePos", "Y")));
            if(owner.GetInventory().GetCount("item_bullet") > 0) {
                if (eventname == "hg_up") {
                    FireBullet(bullet, 45);
                    CreateEffect((string)GameDataManager.instance.GetData(bullet, "Sprites", "light", "up"));
                    CreateEffect((string)GameDataManager.instance.GetData(bullet, "Sprites", "muzzle", "up"));
                }
                else {
                    if (eventname == "hg_down") {
                        CreateEffect((string)GameDataManager.instance.GetData(bullet, "Sprites", "light", "down"));
                        CreateEffect((string)GameDataManager.instance.GetData(bullet, "Sprites", "muzzle", "down"));
                    }
                    else if (eventname == "hg_forward") {
                        CreateEffect((string)GameDataManager.instance.GetData(bullet, "Sprites", "light", "forward"));
                        CreateEffect((string)GameDataManager.instance.GetData(bullet, "Sprites", "muzzle", "forward"));
                    }
                    FireBullet(bullet, 90);
                }
                owner.GetInventory().ModCount("item_bullet", -1);
                if (GameDataManager.instance.GetData(bullet, "Sprites", "muzzleparticles") != null) {
                    Dictionary<string, object> dict = (Dictionary<string, object>)GameDataManager.instance.GetData(bullet, "Sprites", "muzzleparticles");
                    for (int i = 0; i < dict.Count; i++) {
                        string particleName = (string)dict[i.ToString()];
                        ParticleManager.instance.CreateParticle(particleName, transform.position, 0, null, !owner.IsFacingRight());
                    }
                }
            }
        }
        else if(eventname == "attack_basic_finish_fire") {
            SetMuzzlePos(new Vector2((float)GameDataManager.instance.GetData("hg_forward", "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData("hg_forward", "MuzzlePos", "Y")));
            if (owner.GetInventory().GetCount("item_bullet") > 0) {
                FireBullet("bullet_gunkata_finish", 90);
                owner.GetInventory().ModCount("item_bullet", -1);
                CreateEffect("effect_attack_basic_finish");
                if (GameDataManager.instance.GetData(bullet, "Sprites", "muzzleparticles") != null) {
                    Dictionary<string, object> dict = (Dictionary<string, object>)GameDataManager.instance.GetData(bullet, "Sprites", "muzzleparticles");
                    for (int i = 0; i < dict.Count; i++) {
                        string particleName = (string)dict[i.ToString()];
                        ParticleManager.instance.CreateParticle(particleName, transform.position, 0, null, !owner.IsFacingRight());
                    }
                }
            }
        }
    }
}
