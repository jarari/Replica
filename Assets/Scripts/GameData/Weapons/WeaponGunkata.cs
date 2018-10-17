using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 건카타/권총 무기 클래스 */
public class WeaponGunkata : Weapon {
    public override void OnAttack(string eventname) {
        base.OnAttack(eventname);
		JDictionary attackData = GameDataManager.instance.RootData[eventname];

        if (eventname == "hg_forward" || eventname == "hg_down" || eventname == "hg_up") {
            SetMuzzlePos(
				new Vector2(
					attackData["MuzzlePos"]["X"].Value<float>(),
					attackData["MuzzlePos"]["Y"].Value<float>()
				)
			);
        }
    }

    public override void OnWeaponEvent(string eventname) {
        base.OnWeaponEvent(eventname);
		JDictionary attackData = GameDataManager.instance.RootData[eventname];

        if (eventname == "hg_forward" || eventname == "hg_down" || eventname == "hg_up") {
            SetMuzzlePos(
				new Vector2(
					attackData["MuzzlePos"]["X"].Value<float>(),
					attackData["MuzzlePos"]["Y"].Value<float>()
				)
			);

            if(owner.GetInventory().GetCount("item_bullet") > 0) {
				JDictionary bulletSpriteData = GameDataManager.instance.RootData[bullet]["Sprites"];

				if(eventname == "hg_up") {
					FireBullet(bullet, 45);
					CreateEffect(bulletSpriteData["light"]["up"].Value<string>());
					CreateEffect(bulletSpriteData["muzzle"]["up"].Value<string>());
				}
				else if(eventname == "hg_down") {
					FireBullet(bullet, 90);
					CreateEffect(bulletSpriteData["light"]["down"].Value<string>());
					CreateEffect(bulletSpriteData["muzzle"]["down"].Value<string>());
				}
				else if(eventname == "hg_forward") {
					FireBullet(bullet, 90);
					CreateEffect(bulletSpriteData["light"]["forward"].Value<string>());
					CreateEffect(bulletSpriteData["muzzle"]["forward"].Value<string>());
				}

                owner.GetInventory().ModCount("item_bullet", -1);

                if (bulletSpriteData["muzzleparticles"]) {
					foreach(JDictionary particle in bulletSpriteData["muzzleparticles"]) {
						ParticleManager.instance.CreateParticle(particle.Value<string>(), transform.position, 0, null, !owner.IsFacingRight());
					}
                    //Dictionary<string, object> dict = (Dictionary<string, object>)GameDataManager.instance.GetData(bullet, "Sprites", "muzzleparticles");
                    //for (int i = 0; i < dict.Count; i++) {
                    //    string particleName = (string)dict[i.ToString()];
                    //    ParticleManager.instance.CreateParticle(particleName, transform.position, 0, null, !owner.IsFacingRight());
                    //}
                }
            }
        }
        else if(eventname == "attack_basic_finish_fire") {
			JDictionary muzzlePosData = GameDataManager.instance.RootData["hg_forward"]["MuzzlePos"];
			SetMuzzlePos(
				new Vector2(
					muzzlePosData["X"].Value<float>(), 
					muzzlePosData["Y"].Value<float>()
				)
			);

			JDictionary particleData = GameDataManager.instance.RootData[bullet]["Sprites"]["muzzleparticles"];
            if (owner.GetInventory().GetCount("item_bullet") > 0) {
                FireBullet("bullet_gunkata_finish", 90);
                owner.GetInventory().ModCount("item_bullet", -1);
                CreateEffect("effect_attack_basic_finish");
				if(particleData) {
					foreach(JDictionary particle in particleData) {
						ParticleManager.instance.CreateParticle(particle.Value<string>(), transform.position, 0, null, !owner.IsFacingRight());
					}
				}
			}
        }

    }
}
