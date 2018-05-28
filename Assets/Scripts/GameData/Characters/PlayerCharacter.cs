using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PlayerCharacter : Character {
    public override void OnHealthChanged(float delta) {
        if(GetState() != CharacterStates.Uncontrollable) {
            if (delta >= GetMaxStat(CharacterStats.Health) * 0.5f)
                anim.Play("hit_crit");
            else
                anim.Play("hit_normal");
        }
    }

    public override void OnDeath() {
        base.OnDeath();
    }

    public override void KnockDown() {
        anim.Play("hit_down_knockout");
        SetUncontrollable(true);
        SetState(CharacterStates.Uncontrollable);
    }
}
