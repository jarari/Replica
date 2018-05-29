using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 레벨 익스포터에서 배경 스크롤러를 테스트할 때 쓰이는 AI */
class AITestPreviewSceneary : AIBossController {
    protected override void Update() {
        Walk(1);
        maxJump = Mathf.Pow(character.GetCurrentStat(CharacterStats.JumpPower), 2) / 3924f;
        if (Physics2D.OverlapBox((Vector2)transform.position + box.offset + new Vector2(32, -box.size.y / 2f + 16), new Vector2(16, 10), 0, Helper.mapLayer) != null
                            && Physics2D.OverlapBox((Vector2)transform.position + new Vector2(32, maxJump), new Vector2(16, 5), 0, Helper.mapLayer) == null) {
            Jump();
        }
    }
}
