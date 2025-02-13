using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneActor : Character {

    public override void Initialize(string classname) {
        base.Initialize(classname);
        minDistToDash = -1f;
    }

    protected override void OnHealthChanged(float delta) {
        base.OnHealthChanged(delta);
        if (delta < 0) {
            if (GetState() != CharacterStates.Uncontrollable) {
                if (delta >= GetMaxStat(CharacterStats.Health) * 0.5f)
                    anim.Play("hit_crit");
                else {
                    if (!HasFlag(CharacterFlags.UnstoppableAttack))
                        anim.Play("hit_normal");
                }
            }
        }
    }

    public override void KnockDown() {
        anim.Play("hit_down_knockout");
        SetUncontrollable(true);
        SetState(CharacterStates.Uncontrollable);
    }

    protected override void OnAttackEvent(string eventname) {
        base.OnAttackEvent(eventname);
        Weapon wep = null;
        if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("gunkata")) {
            wep = GetWeapon(WeaponTypes.Pistol);
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fist")) {
            wep = GetWeapon(WeaponTypes.Fist);
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("sword")) {
            wep = GetWeapon(WeaponTypes.Sword);
        }
        if (wep != null) {
            lastUsedWeapon = wep;
            basecontroller.ResetAttackTimer();
        }
    }
}
