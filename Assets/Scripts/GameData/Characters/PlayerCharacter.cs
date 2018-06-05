using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 플레이어 캐릭터 클래스 */
public class PlayerCharacter : Character {
    protected override void OnHealthChanged(float delta) {
        base.OnHealthChanged(delta);
        if(delta < 0) {
            if (GetState() != CharacterStates.Uncontrollable) {
                if (delta >= GetMaxStat(CharacterStats.Health) * 0.5f)
                    anim.Play("hit_crit");
                else {
                    if(!HasFlag(CharacterFlags.UnstoppableAttack))
                        anim.Play("hit_normal");
                }
            }
        }
    }

    protected override void OnDeath() {
        base.OnDeath();
    }

    public override void KnockDown() {
        anim.Play("hit_down_knockout");
        SetUncontrollable(true);
        SetState(CharacterStates.Uncontrollable);
    }
}
