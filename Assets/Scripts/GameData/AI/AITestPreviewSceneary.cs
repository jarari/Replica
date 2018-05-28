using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class AITestPreviewSceneary : AIBossController {
    protected override void Update() {
        Walk(1);
        maxJump = Mathf.Pow(character.GetCurrentStat(CharacterStats.JumpPower), 2) / 3924f;
        if (Physics2D.OverlapBox((Vector2)transform.position + box.offset + new Vector2(32, -box.size.y / 2f + 16), new Vector2(16, 10), 0, mapLayer) != null
                            && Physics2D.OverlapBox((Vector2)transform.position + new Vector2(32, maxJump), new Vector2(16, 5), 0, mapLayer) == null) {
            Jump();
        }
    }
}
