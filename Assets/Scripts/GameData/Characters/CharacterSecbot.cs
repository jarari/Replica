using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CharacterSecbot : Character {
    public override void OnHealthChanged(float delta) {
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
