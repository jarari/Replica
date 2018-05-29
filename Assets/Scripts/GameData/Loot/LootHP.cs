using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class LootHP : Loot {
    protected float healAmount = 0;
    public override void Initialize(string classname, int _count) {
        base.Initialize(classname, _count);
        healAmount = Convert.ToSingle(GameDataManager.instance.GetData("Data", classname, "HealAmount"));
    }
    public override void Pickup(Character c) {
        if (c.GetCurrentStat(CharacterStats.Health) == c.GetMaxStat(CharacterStats.Health))
            return;
        c.ModStat(CharacterStats.Health, healAmount);
        base.Pickup(c);
    }
}
