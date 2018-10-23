using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 시큐리티 봇 무기 클래스 */
public class WeaponSecbot : Weapon {
    public override void OnAttack(string eventname) {
        base.OnAttack(eventname);
        if (eventname == "attack_secbot") {
			JDictionary muzzlePosData = GameDataManager.instance.RootData[eventname]["MuzzlePos"];
			SetMuzzlePos(
				new Vector2(
					muzzlePosData["X"].Value<float>(),
					muzzlePosData["Y"].Value<float>()
				)
			);
        }
    }

    public override void OnWeaponEvent(string eventname) {
        base.OnWeaponEvent(eventname);
        if (eventname == "attack_secbot_fire") {
            FireBullet(bullet, 90);
        }
    }
}