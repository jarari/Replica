using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 체술 무기 클래스 */
public class WeaponFist : Weapon {
    public override void OnAttack(string eventname) {
        base.OnAttack(eventname);
        if(eventname == "attack_fist_upper") {
            owner.GetAnimator().SetBool("DiscardFromAnyState", true);
        }
    }

    public override void OnWeaponEvent(string eventname) {
        base.OnWeaponEvent(eventname);
        if (eventname == "attack_fist_upper_jump") {
            owner.AddForce(Vector3.up * owner.GetCurrentStat(CharacterStats.JumpPower) * 1.2f);
        }
    }
}
