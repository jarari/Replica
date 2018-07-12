using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 시큐리티 봇 2 클래스 */
public class CharacterSecbot2 : Character {
    protected override void OnStagger(float stagger) {
        base.OnStagger(stagger);
        if (GetState() != CharacterStates.Uncontrollable) {
            anim.Play("hit");
        }
    }

    protected override void OnDeath() {
        anim.SetTrigger("Death");
        SetUncontrollable(true);
        SetFlag(CharacterFlags.Invincible);
    }

    private void DeathEffect() {
        EffectManager.instance.CreateEffect("effect_character_secbot2_explosion", transform.position, 0);
        ParticleManager.instance.CreateParticle("particle_secbot_death", transform.position, 0);
        ParticleManager.instance.CreateParticle("particle_secbot_death2", transform.position, 0);
        base.OnDeath();
    }

    public override void KnockDown() {
        return;
    }
}
