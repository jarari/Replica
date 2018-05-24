using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacterMovement : MoveObject {

    protected MoveObject movementController;
    protected BoxCollider2D box;
    protected BoxCollider2D secondbox;
    protected float maxJump = 0;
    protected LayerMask groundLayer;
    protected LayerMask mapLayer;
    protected LayerMask ceilingLayer;
    protected float dashCooldown = 1f;
    protected float lastDash = 0;
    protected float dashDir = 1;
    protected float minDistToDash = 0;
    protected float targetApproachRange;
    protected float subX = -1f;

    // Use this for initialization
    public override void Initialize(Character c) {
        base.Initialize(c);
        lastDash = Time.time - dashCooldown;
    }

    // Update is called once per frame
    protected virtual void Update() {
        if (character.GetUncontrollableTimeLeft() > 0) {
            character.SetState(CharacterStates.Uncontrollable);
        }
        else {
            if (character.GetState() == CharacterStates.Uncontrollable)
                character.SetState(CharacterStates.Idle);

            if (character.GetState() == CharacterStates.Jump
                || character.GetState() == CharacterStates.Shift) {
                character.GetAnimator().SetInteger("State", (int)character.GetState());
            }
            else {
                character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
            }

            if (character.GetState() == CharacterStates.Shift) {
                character.ModifyHeight(0.65f);
                Move(dashDir * 0.4f);
            }
            else if(character.GetState() == CharacterStates.Sit) {
                character.ModifyHeight(0.65f);
            }
            else {
                character.ModifyHeight(1f);
            }
        }
    }

    protected void LateUpdate() {

    }

    protected void Walk(int dir) {
        if (character.GetState() == CharacterStates.Uncontrollable)
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Walk);
        if(character.GetState() != CharacterStates.Sit)
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

    protected void Sprint(int dir) {
        if (character.GetState() == CharacterStates.Uncontrollable
            || character.GetState() == CharacterStates.Sit)
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Sprint);
        if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("sprint")) {
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
        if (character.GetState() == CharacterStates.Uncontrollable)
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
    }

    protected bool CanDash() {
        if (character.GetState() == CharacterStates.Uncontrollable)
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
        dashDir = dir;
    }

    protected void OnShiftEvent() {
        lastDash = Time.time;
        if (dashDir == 1) {
            character.FlipFace(true);
        }
        else if (dashDir == -1) {
            character.FlipFace(false);
        }
        character.SetState(CharacterStates.Shift);
        character.SetFlag(CharacterFlags.Invincible);
        character.gameObject.layer = LayerMask.NameToLayer("CharactersShifting");
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnShiftOutEvent() {
        if (character.GetAnimator().GetInteger("State") != (int)CharacterStates.Sprint)
            character.AddUncontrollableTime(0.3f);
        character.SetState(CharacterStates.Idle);
        character.RemoveFlag(CharacterFlags.Invincible);
        character.gameObject.layer = LayerMask.NameToLayer("Characters");
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
    }

    protected void Sit() {
        if (character.GetState() == CharacterStates.Uncontrollable)
            return;
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Sit);
    }

    protected void OnSit() {
        character.SetState(CharacterStates.Sit);
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnStandup() {
        character.SetState(CharacterStates.Idle);
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    protected bool CanGoDown() {
        return true;
    }

    protected void GoDown() {

    }

    protected void Follow(Vector3 pos, float xradius) {
        float dx = pos.x - transform.position.x;
        float dy = pos.y - transform.position.y;
        float dist = Mathf.Abs(dx);
        int dir = (int)Mathf.Sign(dx);
        if (character.GetUncontrollableTimeLeft() == 0) {
            RaycastHit2D rayup = Physics2D.Raycast(transform.position, new Vector2(0, 1), maxJump, groundLayer);
            RaycastHit2D rayunder = Physics2D.Raycast((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f + 33), new Vector2(0, -1), 128f, groundLayer);
            if (Physics2D.OverlapBox((Vector2)transform.position + new Vector2(32 * dir, 0), new Vector2(16, 10), 0, mapLayer) != null
                            && Physics2D.OverlapBox((Vector2)transform.position + new Vector2(32 * dir, maxJump), new Vector2(16, 5), 0, mapLayer) == null) {
                Jump();
            }
            if (dist > xradius) {
                subX = -1;
                if (dist > minDistToDash && CanDash()) {
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
            else {
                //weaponAnim.ResetTrigger("Jump");
            }
        }
        else {
            //weaponAnim.SetInteger("State", 0);
        }
    }
}
