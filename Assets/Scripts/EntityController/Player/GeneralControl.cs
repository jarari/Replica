using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralControl : BasicCharacterMovement {
    private float comboTimer;
    private float comboTime = 0.1f;
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
                if (Input.GetKey(KeyCode.LeftArrow)) {
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
                    else {
                        Dash(1);
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
                    if (Input.GetKey(KeyCode.A)) {
                        //Attack
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
