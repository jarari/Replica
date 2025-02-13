using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISceneActor : AIBaseController {
    protected override void AdditionalData(string aidata) {
        AIEnabled = false;
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    private void OnSpawnEvent() {
    }

    protected override void Attack() {
    }
}
