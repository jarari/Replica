using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacterMovement : MoveObject {

    protected MoveObject movementController;
    protected BoxCollider2D box;
    protected BoxCollider2D nofriction;
    protected float maxJump = 0;
    protected LayerMask groundLayer;
    protected LayerMask mapLayer;
    protected LayerMask ceilingLayer;
    protected float dashCooldown = 0.6f;
    protected float lastDash = 0;
    protected float dashDir = 1;
    protected float minDistToDash = 0;
    protected float targetApproachRange;
    protected float subX = -1f;
    protected float grenadeCharge = 0f;
    protected bool goingdown = false;

    // Use this for initialization
    public override void Initialize(Character c) {
        base.Initialize(c);
        if (GetComponents<BoxCollider2D>().Count() > 0) {
            box = GetComponents<BoxCollider2D>()[0];
            nofriction = GetComponents<BoxCollider2D>()[1];
        }
        lastDash = Time.time - dashCooldown;
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        mapLayer = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Ceiling"));
        ceilingLayer = 1 << LayerMask.NameToLayer("Ceiling");
    }

    // Update is called once per frame
    protected virtual void Update() {
        if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) > 0f) {
            foreach (Collider2D rayvel in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, (box.size.y / 2f + 16f) * Mathf.Sign(GetComponent<Rigidbody2D>().velocity.y)), new Vector2(2000, 8f), 0, groundLayer)) {
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
            foreach (Collider2D rayup in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, (box.size.y / 2f + 16f)), new Vector2(2000, 8f), 0, groundLayer)) {
                if (rayup != null) {
                    Physics2D.IgnoreCollision(rayup, box);
                    Physics2D.IgnoreCollision(rayup, nofriction);
                }
            }
        }

        if (character.GetUncontrollableTimeLeft() == 0 && character.GetState() == CharacterStates.Uncontrollable)
            character.SetState(CharacterStates.Idle);

        if (character.GetAnimator().GetInteger("State") == (int)CharacterStates.Walk
            || character.GetAnimator().GetInteger("State") == (int)CharacterStates.Sprint
            || character.GetAnimator().GetInteger("State") == (int)CharacterStates.Sit) {
            character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
        }

        if (character.GetState() != CharacterStates.Throw) {
            if(grenadeCharge != 0) {
                grenadeCharge = 0;
                OnGrenadeCancelled();
            }
        }
        else {
            grenadeCharge = Mathf.Clamp(grenadeCharge + Time.deltaTime, 0, character.GetCurrentStat(CharacterStats.GrenadeFullCharge));
        }

        /*if (character.GetState() == CharacterStates.Shift) {
            ForceMove(dashDir * 0.6f);
        }*/
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
        if (dir == 1) {
            character.FlipFace(true);
        }
        else if (dir == -1) {
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
            && character.GetState() != CharacterStates.Throw) {
            character.SetState(CharacterStates.Sprint);
            Move(dir);
            if (dir == 1) {
                character.FlipFace(true);
            }
            else if (dir == -1) {
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

    protected void OnLandEvent() {
        character.SetState(CharacterStates.Idle);
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
    }

    protected bool CanDash() {
        if (character.GetUncontrollableTimeLeft() > 0)
            return false;
        if (!character.IsOnGround())
            return false;
        if (Time.time - lastDash < dashCooldown)
            return false;
        return true;
    }

    protected void Dash(int dir) {
        if (!CanDash())
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Shift);
        if(dir == 0) {
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

    protected void OnTumbleEvent() {
        if(character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("tumble_back"))
            EffectManager.instance.CreateEffect("effect_tumble_back", transform.position, (int)Mathf.Sign(transform.localScale.x));
        else if(character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("tumble_front"))
            EffectManager.instance.CreateEffect("effect_tumble_front", transform.position, (int)Mathf.Sign(transform.localScale.x));
        lastDash = Time.time;
        character.AddUncontrollableTime(0.5f);
        ForceMove(dashDir);
        AddForce(Vector3.up * character.GetCurrentStat(CharacterStats.JumpPower) * 0.6f
            + Vector3.right * dashDir * character.GetCurrentStat(CharacterStats.JumpPower) * 0.2f);
        character.SetState(CharacterStates.Shift);
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

    protected void OnSit() {
        ForceMove(0);
        character.SetState(CharacterStates.Sit);
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnStandup() {
        character.SetState(CharacterStates.Idle);
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    protected bool CanGoDown() {
        return Physics2D.OverlapBox((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f + 16f), new Vector2(box.size.x, 16f), 0, ceilingLayer) == null;
    }

    protected void GoDown() {
        goingdown = true;
        StartCoroutine(GoDownEnd());
        foreach (Collider2D raydown in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, -box.size.y / 2f), new Vector2(2000, 64f), 0, groundLayer)) {
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

    protected void OnSitLoop() {
        character.SetState(CharacterStates.Sit);
    }

    protected void OnAttackEvent(string eventname) {
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
    }

    protected void OnWeaponEvent(string eventname) {
        if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("gunkata")) {
            character.GetWeapon(WeaponTypes.Pistol).OnWeaponEvent(eventname);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fist")) {
            character.GetWeapon(WeaponTypes.Fist).OnWeaponEvent(eventname);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("sword")) {
            character.GetWeapon(WeaponTypes.Sword).OnWeaponEvent(eventname);
        }
    }

    protected void OnParentedEffectEvent(string effect) {
        EffectManager.instance.CreateEffect(effect, transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
    }

    protected void OnEffectEvent(string effect) {
        EffectManager.instance.CreateEffect(effect, transform.position, (int)Mathf.Sign(transform.localScale.x));
    }

    protected void OnChargeGrenade() {
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Throw);
        character.SetState(CharacterStates.Throw);
    }

    protected void OnGrenadeCancelled() {
        character.GetAnimator().Play("idle_loop");
        character.RemoveBuff("debuff_ms_chargegrenade");
    }

    protected void OnThrowGrenade(string eventname) {
        //Get grenade class, not yet implemented for now.
        Vector2 throwpos = (Vector2)transform.position + new Vector2((float)GameDataManager.instance.GetData("Data", eventname, "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData("Data", eventname, "MuzzlePos", "Y"));
        float throwang = Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "ThrowAngle"));
        character.GiveWeapon("weapon_grenade");
        Weapon grenade = character.GetWeapon(WeaponTypes.Throwable);
        BulletManager.instance.CreateThrowable("throwable_grenade", throwpos, character, grenade,
            character.GetCurrentStat(CharacterStats.GrenadeThrowPower) * grenadeCharge / character.GetCurrentStat(CharacterStats.GrenadeFullCharge),
            character.GetCurrentStat(grenade, WeaponStats.Range), 90 - (90 - throwang) * character.GetFacingDirection(), 300,
            grenade.GetEssentialStats());
        character.RemoveWeapon(WeaponTypes.Throwable);
    }

    protected void Follow(Vector3 pos, float xradius) {
        float dx = pos.x - transform.position.x;
        float dy = pos.y - transform.position.y;
        float dist = Mathf.Abs(dx);
        int dir = (int)Mathf.Sign(dx);
        maxJump = Mathf.Pow(character.GetCurrentStat(CharacterStats.JumpPower), 2) / 3924f;
        if (character.GetUncontrollableTimeLeft() == 0) {
            RaycastHit2D rayup = Physics2D.Raycast(transform.position, new Vector2(0, 1), maxJump, groundLayer);
            RaycastHit2D rayunder = Physics2D.Raycast((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f + 33), new Vector2(0, -1), 128f, groundLayer);
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
            if (Physics2D.OverlapBox((Vector2)transform.position + new Vector2(32 * dir, 0), new Vector2(16, 10), 0, mapLayer) != null
                            && Physics2D.OverlapBox((Vector2)transform.position + new Vector2(32 * dir, maxJump), new Vector2(16, 5), 0, mapLayer) == null) {
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
