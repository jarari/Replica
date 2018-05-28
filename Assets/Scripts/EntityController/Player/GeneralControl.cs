using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralControl : BasicCharacterMovement {
    private float comboTimer;
    private float comboTime = 0.3f;
    protected KeyCode doubletabKey;
    protected bool waitDoubleTab = false;
    protected bool sprint = false;
    protected override void Update() {
        base.Update();
        comboTimer = Mathf.Clamp(comboTimer - Time.deltaTime, 0, comboTimer);
        if (comboTimer == 0) {
            doubletabKey = KeyCode.None;
            waitDoubleTab = false;
        }
        if (!PlayerPauseUI.IsPaused()) {
            if (character.GetUncontrollableTimeLeft() == 0 && !character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("hit")) {
                if (!character.IsOnGround())
                    sprint = false;

                if (Input.GetKey(KeyCode.RightArrow)) {
                    if (waitDoubleTab && doubletabKey == KeyCode.RightArrow) {
                        sprint = true;
                        Sprint(1);
                    }
                    else {
                        comboTimer = comboTime;
                        if (doubletabKey != KeyCode.RightArrow)
                            waitDoubleTab = false;
                        doubletabKey = KeyCode.RightArrow;
                        if (sprint) {
                            Sprint(1);
                        }
                        else {
                            Walk(1);
                        }
                    }
                }
                else if (Input.GetKey(KeyCode.LeftArrow)) {
                    if (waitDoubleTab && doubletabKey == KeyCode.LeftArrow) {
                        sprint = true;
                        Sprint(-1);
                    }
                    else {
                        comboTimer = comboTime;
                        if (doubletabKey != KeyCode.LeftArrow)
                            waitDoubleTab = false;
                        doubletabKey = KeyCode.LeftArrow;
                        if (sprint) {
                            Sprint(-1);
                        }
                        else {
                            Walk(-1);
                        }
                    }
                }

                if(!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)){
                    sprint = false;
                    if (comboTimer > 0 &&
                     (doubletabKey == KeyCode.RightArrow || doubletabKey == KeyCode.LeftArrow))
                        waitDoubleTab = true;
                }

                if (Input.GetKeyDown(KeyCode.LeftShift)) {
                    if (Input.GetKey(KeyCode.LeftArrow)) {
                        Dash(-1);
                    }
                    else if(Input.GetKey(KeyCode.RightArrow)){
                        Dash(1);
                    }
                    else {
                        Dash(0);
                    }
                }

                if (character.GetState() != CharacterStates.Attack
                    && character.GetState() != CharacterStates.Throw) {
                    if (Input.GetKey(KeyCode.DownArrow)) {
                        Sit();
                    }

                    if (Input.GetKeyDown(KeyCode.Space)) {
                        if (Input.GetKey(KeyCode.DownArrow)) {
                            if (CanGoDown()) {
                                GoDown();
                            }
                        }
                        else {
                            Jump();
                        }
                    }
                }
                else if (character.GetState() == CharacterStates.Attack
                    && Input.GetKey(KeyCode.DownArrow)
                    && character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsName("HG Continue"))
                    Sit();

                if (character.IsOnGround() 
                    && character.GetState() != CharacterStates.Jump
                    && character.GetState() != CharacterStates.Shift) {
                    if (Input.GetKeyDown(KeyCode.X)) {
                        if (character.GetState() != CharacterStates.Attack)
                            character.GetAnimator().Play("Attack Junction");
                        character.GetAnimator().ResetTrigger("UpX");
                        character.GetAnimator().ResetTrigger("FlatX");
                        character.GetAnimator().ResetTrigger("DownX");
                        if (Input.GetKey(KeyCode.DownArrow)) {
                            character.GetAnimator().SetTrigger("DownX");
                        }
                        else if (Input.GetKey(KeyCode.UpArrow)) {
                            character.GetAnimator().SetTrigger("UpX");
                        }
                        else {
                            character.GetAnimator().SetTrigger("FlatX");
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.Z)) {
                        if (character.GetState() != CharacterStates.Attack)
                            character.GetAnimator().Play("HG Junction");
                        character.GetAnimator().ResetTrigger("UpZ");
                        character.GetAnimator().ResetTrigger("FlatZ");
                        character.GetAnimator().ResetTrigger("DownZ");
                        if (Input.GetKey(KeyCode.DownArrow)) {
                            character.GetAnimator().SetTrigger("DownZ");
                        }
                        else if (Input.GetKey(KeyCode.UpArrow)) {
                            character.GetAnimator().SetTrigger("UpZ");
                        }
                        else {
                            character.GetAnimator().SetTrigger("FlatZ");
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.G)) {
                        if(character.GetState() != CharacterStates.Throw) {
                            character.GetAnimator().Play("throw_ready");
                        }
                    }
                    if (Input.GetKeyUp(KeyCode.G) && character.GetState() == CharacterStates.Throw) {
                        character.GetAnimator().ResetTrigger("UpG");
                        character.GetAnimator().ResetTrigger("FlatG");
                        character.GetAnimator().ResetTrigger("DownG");
                        if (Input.GetKey(KeyCode.DownArrow)) {
                            character.GetAnimator().SetTrigger("DownG");
                        }
                        else if (Input.GetKey(KeyCode.UpArrow)) {
                            character.GetAnimator().SetTrigger("UpG");
                        }
                        else {
                            character.GetAnimator().SetTrigger("FlatG");
                        }
                    }
                }
                else {
                    if (Input.GetKeyDown(KeyCode.X)) {
                        if (character.GetState() != CharacterStates.Attack)
                            character.GetAnimator().Play("Attack Junction");
                        character.GetAnimator().ResetTrigger("UpX");
                        character.GetAnimator().ResetTrigger("FlatX");
                        character.GetAnimator().ResetTrigger("DownX");
                        if (Input.GetKey(KeyCode.DownArrow)) {
                            character.GetAnimator().SetTrigger("DownX");
                        }
                        else if (Input.GetKey(KeyCode.UpArrow)) {
                            character.GetAnimator().SetTrigger("UpX");
                        }
                        else {
                            character.GetAnimator().SetTrigger("FlatX");
                        }
                    }
                }
                if(character.GetState() == CharacterStates.Attack) {
                    if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.85f) {
                        if (Input.GetKey(KeyCode.LeftArrow))
                            character.FlipFace(false);
                        else if (Input.GetKey(KeyCode.RightArrow))
                            character.FlipFace(true);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
                PlayerPauseUI.PauseToggle();
        }
        else {
            if (Input.GetKeyDown(KeyCode.Escape))
                PlayerPauseUI.GoBack();
        }
    }
}
