using System;
using System.Collections;
using UnityEngine;

public class AIBossController : BasicCharacterMovement {

    protected string className;
    protected int currentPhase = 0;
    protected float restingTime = 3f;
    protected float nextActive;
    protected Character target;
    protected bool AIEnabled = true;
    public virtual void Initialize(Character c, string aidata) {
        base.Initialize(c);
        className = aidata;
        currentPhase = 0;
        restingTime = (float)GameDataManager.instance.GetData("Data", aidata, "RestingTime");
        targetApproachRange = character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.Range) * 0.5f;
        target = CharacterManager.instance.GetPlayer();
        nextActive = Time.realtimeSinceStartup;
        AIEnabled = true;
        groundLayer = (1 << LayerMask.NameToLayer("Ground"));
        AdditionalData();
    }

    protected virtual void AdditionalData() {

    }

    public void SetAIStatus(bool stat) {
        AIEnabled = stat;
    }

    protected override void Update() {
        base.Update();
        target = CharacterManager.instance.GetPlayer();
        if (AIEnabled && nextActive <= Time.realtimeSinceStartup && target != null)
            Pattern();
    }

    protected virtual void Pattern() {

    }
}
