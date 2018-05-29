using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* 시큐리티 봇 클래스 */
public class CharacterSecbot : Character {
    public override void OnStagger(float stagger) {
        if (GetState() != CharacterStates.Uncontrollable) {
            anim.Play("hit");
        }
    }

    public override void OnDeath() {
        EffectManager.instance.CreateEffect("effect_character_secbot_death", transform.position, GetFacingDirection());
        EffectManager.instance.CreateEffect("effect_character_secbot_explosion", transform.position, 0);
        base.OnDeath();
    }

    public override void KnockDown() {
        return;
    }
}
