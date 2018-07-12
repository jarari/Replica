using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/* 모든 캐릭터 움직임의 기본이 되는 컨트롤러 */
public class BasicCharacterMovement : MoveObject {

    protected MoveObject movementController;
    protected BoxCollider2D box;
    protected BoxCollider2D nofriction;
    protected float maxJump = 0;
    protected float dashCooldown = 0.6f;
    protected float lastDash = 0;
    protected float dashDir = 1;
    protected float minDistToDash = 0;
    protected float targetApproachRange;
    protected float subX = -1f;
    protected float grenadeCharge = 0f;
    protected float grenadeChargeRatio = 0f;
    protected float grenadeThrowMin = 0.25f;
    protected bool goingdown = false;

    // Use this for initialization
    public override void Initialize(Character c) {
        base.Initialize(c);
        if (GetComponents<BoxCollider2D>().Count() > 0) {
            box = GetComponents<BoxCollider2D>()[0];
            nofriction = GetComponents<BoxCollider2D>()[1];
        }
        lastDash = Time.time - dashCooldown;
    }

    // Update is called once per frame
    protected virtual void Update() {
        /* 위, 아래로 자유롭게 이동할 수 있도록 윗쪽 블럭과의 충돌을 미리 꺼놓음 */
        if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) > 0f) {
            foreach (Collider2D rayvel in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, (box.size.y / 2f + 16f) * Mathf.Sign(GetComponent<Rigidbody2D>().velocity.y)), new Vector2(2000, 8f), 0, Helper.groundLayer)) {
                if (rayvel != null) {
                    if (GetComponent<Rigidbody2D>().velocity.y > 0) {
                        Physics2D.IgnoreCollision(rayvel, box);
                        Physics2D.IgnoreCollision(rayvel, nofriction);
                    }
                    else if (!goingdown) {
                        Physics2D.IgnoreCollision(rayvel, box, false);
                        Physics2D.IgnoreCollision(rayvel, nofriction, false);
                    }
                }
            }
        }
        else {
            foreach (Collider2D rayup in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, (box.size.y / 2f + 16f)), new Vector2(2000, 8f), 0, Helper.groundLayer)) {
                if (rayup != null) {
                    Physics2D.IgnoreCollision(rayup, box);
                    Physics2D.IgnoreCollision(rayup, nofriction);
                }
            }
        }

        /* 조종 불가상태에서 돌아왔을 때 Idle 실행*/
        if (character.GetUncontrollableTimeLeft() == 0 && character.GetState() == CharacterStates.Uncontrollable) {
            character.SetState(CharacterStates.Idle);
            character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
        }

        /* 키보드 입력을 유지해야하는 동작들 (걷기, 달리기, 앉기)은 매 프레임마다 Idle로 리셋 */
        if (character.GetAnimator().GetInteger("State") == (int)CharacterStates.Walk
            || character.GetAnimator().GetInteger("State") == (int)CharacterStates.Sprint
            || character.GetAnimator().GetInteger("State") == (int)CharacterStates.Sit) {
            character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
        }

        /* 투척 동작 관련
         * 투척 관련 자세가 아닌데 수류탄 게이지가 올라가있으면 캔슬당한 것으로 판정 */
        if (character.GetState() != CharacterStates.Throw) {
            if(grenadeCharge != 0) {
                OnGrenadeCancelled();
            }
        }
        else {
            grenadeCharge = Mathf.Clamp(grenadeCharge + Time.deltaTime, 0, character.GetCurrentStat(CharacterStats.GrenadeFullCharge));
            grenadeChargeRatio = Mathf.Clamp(grenadeCharge / character.GetCurrentStat(CharacterStats.GrenadeFullCharge)
            , grenadeThrowMin, 1);
        }

        
        /* 앉았을 때 캐릭터 히트박스 감소 */
        if(character.GetState() == CharacterStates.Sit) {
            character.ModifyHeight(0.65f);
        }
        else {
            character.ModifyHeight(1f);
        }
    }

    protected void LateUpdate() {

    }

    protected void Walk(float dir) {
        if (character.GetUncontrollableTimeLeft() > 0
            || character.GetState() == CharacterStates.Attack)
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Walk);
        if(character.GetState() != CharacterStates.Sit
            && character.GetState() != CharacterStates.Throw)
            Move(dir * 0.6f);
        if (dir > 0) {
            character.FlipFace(true);
        }
        else if (dir < 0) {
            character.FlipFace(false);
        }
        if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("walk")) {
            character.SetState(CharacterStates.Walk);
        }
    }

    protected void Sprint(float dir) {
        if (character.GetUncontrollableTimeLeft() > 0
            || character.GetState() == CharacterStates.Attack)
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Sprint);
        if (character.GetState() != CharacterStates.Sit
            && character.GetState() != CharacterStates.Throw
            && character.IsOnGround()) {
            character.SetState(CharacterStates.Sprint);
            Move(dir);
            if (dir > 0) {
                character.FlipFace(true);
            }
            else if (dir < 0) {
                character.FlipFace(false);
            }
        }
        else if(character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("jump") || character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fall")) {
            Move(dir * 0.6f);
            if (dir == 1) {
                character.FlipFace(true);
            }
            else if (dir == -1) {
                character.FlipFace(false);
            }
        }
    }
    

    protected bool CanJump() {
        if (character.GetUncontrollableTimeLeft() > 0)
            return false;
        if (!character.IsOnGround())
            return false;
        return true;
    }

    protected void Jump() {
        if (!CanJump())
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Jump);
    }

    protected void OnJumpEvent() {
        character.SetState(CharacterStates.Jump);
        AddForce(Vector3.up * character.GetCurrentStat(CharacterStats.JumpPower));
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnFallEvent() {
        character.SetState(CharacterStates.Jump);
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Jump);
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnLandEvent() {
        character.SetState(CharacterStates.Idle);
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
    }

    /* 이름이 대쉬인데 덤블링임 사실 */
    protected bool CanDash() {
        if (character.GetUncontrollableTimeLeft() > 0)
            return false;
        if (!character.IsOnGround())
            return false;
        if (Time.time - lastDash < dashCooldown)
            return false;
        return true;
    }

    /* 방향이 입력이 없다면 백덤블링
     * 있다면 해당 방향으로 덤블링 */
    protected void Dash(int dir) {
        if (!CanDash())
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Shift);
        character.SetState(CharacterStates.Shift);
        if (dir == 0) {
            dashDir = (Convert.ToSingle(character.IsFacingRight()) - 0.5f) * -2f;
            character.GetAnimator().Play("tumble_back");
        }
        else {
            dashDir = dir;
            if (dashDir == 1) {
                character.FlipFace(true);
            }
            else if (dashDir == -1) {
                character.FlipFace(false);
            }
            character.GetAnimator().Play("tumble_front");
        }
    }

    /* 덤블링 높이는 점프력에 비례함 */
    protected void OnTumbleEvent() {
        if(character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("tumble_back"))
        {
            EffectManager.instance.CreateEffect("effect_tumble_back", transform.position, (int)Mathf.Sign(transform.localScale.x));
            EffectManager.instance.CreateEffect("effect_tumble_back2", transform.position, (int)Mathf.Sign(transform.localScale.x));
            EffectManager.instance.CreateEffect("effect_tumble_backf", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
            ParticleManager.instance.CreateParticle("particle_tumb", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
        }
        else if(character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("tumble_front"))
        {
            EffectManager.instance.CreateEffect("effect_tumble_front", transform.position, (int)Mathf.Sign(transform.localScale.x));
            EffectManager.instance.CreateEffect("effect_tumble_front2", transform.position, (int)Mathf.Sign(transform.localScale.x));
            EffectManager.instance.CreateEffect("effect_tumble_frontf", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
            ParticleManager.instance.CreateParticle("particle_tumb", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
        }

        lastDash = Time.time;
        character.AddUncontrollableTime(0.25f);
        ForceMove(dashDir);
        AddForce(Vector3.up * character.GetCurrentStat(CharacterStats.JumpPower) * 0.15f
            + Vector3.right * dashDir * character.GetCurrentStat(CharacterStats.JumpPower) * 0.7f);
        character.SetFlag(CharacterFlags.Invincible);
        character.gameObject.layer = LayerMask.NameToLayer("CharactersShifting");
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnTumbleInvincibilityEndEvent() {
        character.RemoveFlag(CharacterFlags.Invincible);
        character.gameObject.layer = LayerMask.NameToLayer("Characters");
    }

    protected void OnTumbleEndEvent() {
        character.SetState(CharacterStates.Idle);
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
    }

    protected void Sit() {
        if (character.GetState() == CharacterStates.Uncontrollable)
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Sit);
    }

    /* 앉으면 가속도 초기화 (슬라이딩 방지)*/
    protected void OnSit() {
        ForceMove(0);
        character.SetState(CharacterStates.Sit);
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnSitLoop() {
        character.SetState(CharacterStates.Sit);
    }

    protected void OnStandup() {
        character.SetState(CharacterStates.Idle);
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    /* 맵 레이어는 Ground와 Ceiling으로 나뉘는데,
     * Ceiling은 어떤 상황에서도 뚫리지 않는 지형
     * Ground는 자유자재로 위아래로 돌아다닐 수 있는 지형 */
    protected bool CanGoDown() {
        return Physics2D.OverlapBox((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f + 16f), new Vector2(box.size.x, 16f), 0, Helper.ceilingLayer) == null;
    }

    protected void GoDown() {
        goingdown = true;
        StartCoroutine(GoDownEnd());
        foreach (Collider2D raydown in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, -box.size.y / 2f), new Vector2(2000, 64f), 0, Helper.groundLayer)) {
            if (raydown != null) {
                Physics2D.IgnoreCollision(raydown, box);
                Physics2D.IgnoreCollision(raydown, nofriction);
            }
        }
    }
    protected IEnumerator GoDownEnd() {
        yield return new WaitWhile(() => character.IsOnGround());
        yield return new WaitWhile(() => !character.IsOnGround() || GetComponentInParent<Rigidbody2D>().velocity.y < -1f);
        goingdown = false;
    }

    protected void OnIdle() {
        character.SetState(CharacterStates.Idle);
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    /* 공격 이벤트 */
    protected virtual void OnAttackEvent(string eventname) {
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Attack);
        character.SetState(CharacterStates.Attack);
        if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("gunkata")) {
            character.GetWeapon(WeaponTypes.Pistol).OnAttack(eventname);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fist")) {
            character.GetWeapon(WeaponTypes.Fist).OnAttack(eventname);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("sword")) {
            character.GetWeapon(WeaponTypes.Sword).OnAttack(eventname);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("ai")) {
            character.GetWeapon(WeaponTypes.AI).OnAttack(eventname);
        }
    }

    /* 기타 무기 이벤트 (타이밍에 맞게 특정 행동을 하기 위함) */
    protected virtual void OnWeaponEvent(string eventname) {
        if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("gunkata")) {
            character.GetWeapon(WeaponTypes.Pistol).OnWeaponEvent(eventname);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fist")) {
            character.GetWeapon(WeaponTypes.Fist).OnWeaponEvent(eventname);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("sword")) {
            character.GetWeapon(WeaponTypes.Sword).OnWeaponEvent(eventname);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("ai")) {
            character.GetWeapon(WeaponTypes.AI).OnWeaponEvent(eventname);
        }
    }

    /* 캐릭터에 부착된 이펙트 생성 (애니메이션 이벤트용) */
    protected void OnParentedEffectEvent(string effect) {
        EffectManager.instance.CreateEffect(effect, transform.position, character.GetFacingDirection(), transform);
    }

    /* 캐릭터와 독립된 이펙트 생성 (애니메이션 이벤트용) */
    protected void OnEffectEvent(string effect) {
        EffectManager.instance.CreateEffect(effect, transform.position, character.GetFacingDirection());
    }

    protected void OnChargeGrenade() {
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Throw);
        character.SetState(CharacterStates.Throw);
    }

    protected void OnGrenadeCancelled() {
        if (character.GetState() == CharacterStates.Throw)
            character.GetAnimator().Play("idle_loop");
        grenadeCharge = 0;
        grenadeChargeRatio = 0;
    }

    /* 수류탄 투척 함수.
     * 현재는 무조건 기본 수류탄이 나가도록 돼있지만
     * 차후 인벤토리에서 수류탄 갯수를 확인하여 해당 수류탄을 던질 수 있도록 바꿀 계획 */
    protected void OnThrowGrenade(string eventname) {
        //Get grenade class, not yet implemented for now.
        Vector2 throwpos = (Vector2)transform.position + new Vector2((float)GameDataManager.instance.GetData(eventname, "MuzzlePos", "X") * character.GetFacingDirection()
                                        , (float)GameDataManager.instance.GetData(eventname, "MuzzlePos", "Y"));
        float throwang = Convert.ToSingle(GameDataManager.instance.GetData(eventname, "ThrowAngle"));
        character.GiveWeapon("weapon_grenade");
        Weapon grenade = character.GetWeapon(WeaponTypes.Throwable);
        BulletManager.instance.CreateThrowable("throwable_grenade", throwpos, character, grenade,
            character.GetCurrentStat(CharacterStats.GrenadeThrowPower) * grenadeChargeRatio,
            character.GetCurrentStat(grenade, WeaponStats.ExplosionRadius), 90 - (90 - throwang) * character.GetFacingDirection(), 300,
            grenade.GetEssentialStats());
        character.RemoveWeapon(WeaponTypes.Throwable);
    }

    /* 피격 이벤트 */
    protected virtual void OnHitEvent(int invincible) {
        character.GetAnimator().SetInteger("State", 8);
        character.SetState(CharacterStates.Uncontrollable);
        if (invincible == 1)
            character.SetFlag(CharacterFlags.Invincible);
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
        if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("knockout"))
            StartCoroutine(OnKnockoutRecoverEvent(invincible));
        else
            StartCoroutine(OnHitRecoverEvent(character.GetUncontrollableTimeLeft() + 0.25f, invincible));
    }

    /* 피격 회복 이벤트 */
    protected IEnumerator OnHitRecoverEvent(float delay, int invincible) {
        yield return new WaitForSeconds(delay);
        if(invincible == 1)
            character.RemoveFlag(CharacterFlags.Invincible);
        if(character.GetAnimator().GetInteger("State") == 8)
            character.GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    protected IEnumerator OnKnockoutRecoverEvent(int invincible) {
        yield return new WaitWhile(() => character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("knockout"));
        character.SetUncontrollable(false);
        if (invincible == 1)
            character.RemoveFlag(CharacterFlags.Invincible);
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    /* 다양한 용도로 쓸 수 있는 좌표를 향해 가기 함수
     * Navmesh같은걸 이용한다면 좀 더 멋드러지고 효과적이겠지만
     * 일단은 이 알고리즘을 쓰도록 함. */
    protected void Follow(Vector3 pos, float xradius) {
        float dx = pos.x - transform.position.x;
        float dy = pos.y - transform.position.y;
        float dist = Mathf.Abs(dx);
        int dir = (int)Mathf.Sign(dx);
        maxJump = Mathf.Pow(character.GetCurrentStat(CharacterStats.JumpPower), 2) / 3924f;
        if (character.GetUncontrollableTimeLeft() == 0) {
            character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
            RaycastHit2D rayup = Physics2D.Raycast(transform.position, new Vector2(0, 1), maxJump, Helper.groundLayer);
            RaycastHit2D rayunder = Physics2D.Raycast((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f + 33), new Vector2(0, -1), 128f, Helper.groundLayer);
            if (dist > xradius) {
                subX = -1;
                if (minDistToDash != -1 && dist > minDistToDash && CanDash()) {
                    Dash(dir);
                }
                else {
                    Walk(dir);
                }
            }
            else {
                if (dy > 1f) {
                    if (rayup.collider == null) {
                        if (subX == -1) {
                            float minXDist = 999999;
                            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Ground")) {
                                float odx = obj.transform.position.x - transform.position.x;
                                float ody = obj.transform.position.y - transform.position.y;
                                if (ody < 0 || ody > maxJump || odx * dx < 0 || Mathf.Abs(odx) >= 600)
                                    continue;
                                else {
                                    if (minXDist > Mathf.Abs(odx)) {
                                        subX = obj.transform.position.x;
                                        minXDist = Mathf.Abs(odx);
                                    }
                                }
                            }
                        }
                    }
                    else if (subX != -1) {
                        if (Mathf.Abs(subX - transform.position.x) < 3f) {
                            subX = -1;
                            return;
                        }
                        dir = (int)Mathf.Sign(subX - transform.position.x);
                        Walk(dir);
                    }
                    else {
                        Walk(dir);
                    }
                }
                else if (dy < -1f) {
                    if (rayunder.collider == null && subX == -1) {
                        float minXDist = 999999;
                        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Ground")) {
                            float odx = obj.transform.position.x - transform.position.x;
                            float ody = obj.transform.position.y - transform.position.y;
                            if (ody > -32 || odx * dx < 0 || Mathf.Abs(odx) >= 600)
                                continue;
                            else {
                                if (minXDist > Mathf.Abs(odx)) {
                                    subX = obj.transform.position.x;
                                    minXDist = Mathf.Abs(odx);
                                }
                            }
                        }
                    }
                    else if (subX != -1) {
                        if (Mathf.Abs(subX - transform.position.x) < 3f) {
                            subX = -1;
                            return;
                        }
                        dir = (int)Mathf.Sign(subX - transform.position.x);
                        Walk(dir);
                    }
                    else {
                        Walk(dir);
                    }
                }
            }
            if (Physics2D.OverlapBox((Vector2)transform.position + box.offset + new Vector2(box.size.x / 2f * character.GetFacingDirection(), 16 - box.size.y / 2f), new Vector2(16, 5), 0, Helper.mapLayer) != null
                            && Physics2D.OverlapBox((Vector2)transform.position + box.offset + new Vector2(box.size.x / 2f * character.GetFacingDirection(), maxJump - box.size.y / 2f), new Vector2(16, 5), 0, Helper.mapLayer) == null) {
                Jump();
            }
            if (Mathf.Abs(dy) > 32f && character.IsOnGround() && character.GetState() != CharacterStates.Jump) {
                if (dy < -32) {
                    if (CanGoDown()) {
                        GoDown();
                    }
                }
                else if (rayup.collider != null && dy > 32) {
                    Jump();
                    subX = -1;
                }
            }
        }
        else {
            character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
        }
    }
}
