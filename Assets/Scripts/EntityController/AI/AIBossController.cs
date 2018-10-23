using System;
using System.Collections;
using UnityEngine;

/* 보스급 AI를 위한 클래스
 * 이 클래스는 쓰지 않고 추가적으로 생성한 클래스에서 이 클래스를 오버라이드하여 사용 */

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
        restingTime = (float)GameDataManager.instance.GetData(aidata, "RestingTime");
        targetApproachRange = character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.Range) * 0.5f;
        target = CharacterManager.instance.GetPlayer();
        nextActive = Time.realtimeSinceStartup;
        AIEnabled = true;
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
