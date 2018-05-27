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
            if (character.GetUncontrollableTimeLeft() == 0) {
                if (Input.GetKey(KeyCode.RightArrow)) {
                    if (waitDoubleTab && doubletabKey == KeyCode.RightArrow) {
                        sprint = true;
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

                if (character.IsOnGround() && character.GetState() != CharacterStates.Jump) {
                    if (Input.GetKeyDown(KeyCode.X)) {
                        if(character.GetState() != CharacterStates.Attack)
                            character.GetAnimator().Play("Attack Junction");
                        character.GetAnimator().ResetTrigger("UpX");
                        character.GetAnimator().ResetTrigger("FlatX");
                        character.GetAnimator().ResetTrigger("DownX");
                        if (Input.GetKey(KeyCode.DownArrow)) {
                            character.GetAnimator().SetTrigger("DownX");
                        }
                        else if(Input.GetKey(KeyCode.UpArrow)){
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
