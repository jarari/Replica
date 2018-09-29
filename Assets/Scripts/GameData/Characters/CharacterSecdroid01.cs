using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CharacterSecdroid01 : Character {
    protected override void OnStagger(float stagger) {
        base.OnStagger(stagger);
        if (GetState() != CharacterStates.Uncontrollable) {
            if(IsOnGround())
                anim.Play("hit");
            else
                anim.Play("hit_air");
        }
    }

    protected override void OnDeath() {
        anim.SetTrigger("Death");
        SetUncontrollable(true);
        SetFlag(CharacterFlags.Invincible);
        //ParticleManager.instance.CreateParticle("secdroiddeath", transform.position, Convert.ToSingle(!facingRight) * 180);
        EffectManager.instance.CreateEffect("effect_character_secdroid01_death", transform.position, 0, null, !IsFacingRight());
        EffectManager.instance.CreateEffect("effect_character_secdroid01_explosion", transform.position, 0, null, !IsFacingRight());
        base.OnDeath();
    }

    public override void KnockDown() {
        return;
    }
}