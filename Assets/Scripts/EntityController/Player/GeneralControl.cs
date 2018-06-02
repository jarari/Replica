using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 플레이어 조작 */
public class GeneralControl : BasicCharacterMovement {
    private float sprintDoubleTabTimer = 0;
    private float sprintDoubleTabTime = 0.3f;
    private float attackPostComboTimer = 0;
    private float attackPostComboTime = 0.075f;
    private ComboData currentCombo;
    private List<KeyCombo> keyCombos = new List<KeyCombo>();
    private List<KeyCombo> queuedCombos = new List<KeyCombo>();
    private KeyCombo keyLocked = KeyCombo.None;
    private KeyCode comboLeft = KeyCode.LeftArrow;
    private KeyCode comboRight = KeyCode.RightArrow;
    private float keyComboTimer = 0;
    private float keyComboTime = 0.2f;
    private float nextAttack = 0;
    protected KeyCode doubletabKey;
    protected bool waitDoubleTab = false;
    protected bool sprint = false;

    private void EvaluateCombo(List<KeyCombo> kc) {
        if (nextAttack >= Time.time || character.GetState() == CharacterStates.Shift)
            return;
        Debug.Log("Eval");
        if (character.GetState() != CharacterStates.Attack || currentCombo == null) {
            foreach(KeyValuePair<string, ComboData> kvp in GameDataManager.instance.GetBasicComboData()) {
                if ((character.IsOnGround() && !kvp.Value.isJumpAttack)
                    || (!character.IsOnGround() && kvp.Value.isJumpAttack)) {
                    int i = 0;
                    while (kvp.Value.keyCombos[0] != kc[i] && i < kc.Count - 1) {
                        i++;
                    }
                    bool match = true;
                    for (int j = 0; j < kvp.Value.keyCombos.Count; j++) {
                        if (kvp.Value.keyCombos[j] != kc[i]) {
                            match = false;
                            break;
                        }
                        i++;
                    }
                    if (match) {
                        currentCombo = kvp.Value;
                        character.GetAnimator().Play(kvp.Key);
                        nextAttack = Time.time + 0.25f;
                    }
                }
            }
        }
        else {
            foreach (string nextCombo in currentCombo.nextPossibleCombos) {
                ComboData next = GameDataManager.instance.GetContinuousComboData()[nextCombo];
                if ((character.IsOnGround() && !next.isJumpAttack)
                    || (!character.IsOnGround() && next.isJumpAttack)) {
                    int i = 0;
                    while (next.keyCombos[0] != kc[i] && i < kc.Count - 1) {
                        i++;
                    }
                    bool match = true;
                    for (int j = 0; j < next.keyCombos.Count; j++) {
                        if (next.keyCombos[j] != kc[i]) {
                            match = false;
                            break;
                        }
                        i++;
                    }
                    if (match) {
                        Debug.Log(nextCombo);
                        currentCombo = next;
                        character.GetAnimator().Play(nextCombo);
                        nextAttack = Time.time + 0.25f;
                    }
                }
            }
        }
    }

    protected override void OnAttackEvent(string eventname) {
        base.OnAttackEvent(eventname);
        Weapon wep = null;
        if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("gunkata")) {
            wep = character.GetWeapon(WeaponTypes.Pistol);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fist")) {
            wep = character.GetWeapon(WeaponTypes.Fist);
        }
        else if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("sword")) {
            wep = character.GetWeapon(WeaponTypes.Sword);
        }
        if (wep != null) {
            attackPostComboTimer = 0;
            nextAttack = Time.time + 1f / character.GetCurrentStat(wep, WeaponStats.AttackSpeed);
        }
    }

    protected override void Update() {
        base.Update();
        if (sprintDoubleTabTimer != 0)
            sprintDoubleTabTimer = Mathf.Clamp(sprintDoubleTabTimer - Time.deltaTime, 0, sprintDoubleTabTimer);
        if (sprintDoubleTabTimer == 0) {
            doubletabKey = KeyCode.None;
            waitDoubleTab = false;
        }
        if (nextAttack < Time.time && queuedCombos.Count > 0) {
            EvaluateCombo(queuedCombos);
            keyLocked = KeyCombo.None;
            queuedCombos.Clear();
        }
        if (keyComboTimer != 0)
            keyComboTimer = Mathf.Clamp(keyComboTimer - Time.deltaTime, 0, keyComboTimer);
        if (keyComboTimer == 0 && keyCombos.Count > 0) {
            keyCombos.Clear();
            keyLocked = KeyCombo.None;
        }
        if (character.GetState() == CharacterStates.Attack) {
            if (character.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && nextAttack < Time.time) {
                if (attackPostComboTimer < attackPostComboTime) {
                    attackPostComboTimer = Mathf.Clamp(attackPostComboTimer + Time.deltaTime, 0, attackPostComboTime);
                }
                else {
                    character.GetAnimator().SetTrigger("CancelAttack");
                }
            }
        }
        else if (character.GetState() != CharacterStates.Attack) {
            if (attackPostComboTimer > 0) {
                character.GetAnimator().ResetTrigger("CancelAttack");
                attackPostComboTimer = 0;
            }
            if (character.HasFlag(CharacterFlags.UnstoppableAttack)) {
                character.RemoveFlag(CharacterFlags.UnstoppableAttack);
            }
        }
        if (!PlayerPauseUI.IsPaused()) {
            if (character.GetUncontrollableTimeLeft() == 0) {

                if (Input.GetKey(KeyCode.RightArrow)) {
                    if (waitDoubleTab && doubletabKey == KeyCode.RightArrow) {
                        sprint = true;
                        Sprint(1);
                    }
                    else {
                        sprintDoubleTabTimer = sprintDoubleTabTime;
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
                        sprintDoubleTabTimer = sprintDoubleTabTime;
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
                    if (sprintDoubleTabTimer > 0 &&
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

                if(keyCombos.Count == 0) {
                    comboLeft = KeyCode.LeftArrow;
                    comboRight = KeyCode.RightArrow;
                    if (!character.IsFacingRight()) {
                        comboLeft = comboRight;
                        comboRight = KeyCode.LeftArrow;
                    }
                }
                if (Input.GetKey(comboLeft)) {
                    KeyCombo combo = KeyCombo.Left;
                    keyComboTimer = keyComboTime;
                    if (Input.GetKey(KeyCode.UpArrow)) {
                        combo |= KeyCombo.Up;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow)) {
                        combo |= KeyCombo.Down;
                    }
                    if(keyCombos.Count == 0 || combo != keyCombos[keyCombos.Count - 1])
                        keyCombos.Add(combo);
                    keyLocked = combo;
                }
                if (Input.GetKey(KeyCode.UpArrow)) {
                    KeyCombo combo = KeyCombo.Up;
                    keyComboTimer = keyComboTime;
                    if (Input.GetKey(comboLeft)) {
                        combo |= KeyCombo.Left;
                    }
                    else if (Input.GetKey(comboRight)) {
                        combo |= KeyCombo.Right;
                    }
                    if (keyCombos.Count == 0 || combo != keyCombos[keyCombos.Count - 1])
                        keyCombos.Add(combo);
                    keyLocked = combo;
                }
                if (Input.GetKey(comboRight)) {
                    KeyCombo combo = KeyCombo.Right;
                    keyComboTimer = keyComboTime;
                    if (Input.GetKey(KeyCode.UpArrow)) {
                        combo |= KeyCombo.Up;
                    }
                    else if (Input.GetKey(KeyCode.DownArrow)) {
                        combo |= KeyCombo.Down;
                    }
                    if (keyCombos.Count == 0 || combo != keyCombos[keyCombos.Count - 1])
                        keyCombos.Add(combo);
                    keyLocked = combo;
                }
                if (Input.GetKey(KeyCode.DownArrow)) {
                    KeyCombo combo = KeyCombo.Down;
                    keyComboTimer = keyComboTime;
                    if (Input.GetKey(comboLeft)) {
                        combo |= KeyCombo.Left;
                    }
                    else if (Input.GetKey(comboRight)) {
                        combo |= KeyCombo.Right;
                    }
                    if (keyCombos.Count == 0 || combo != keyCombos[keyCombos.Count - 1])
                        keyCombos.Add(combo);
                    keyLocked = combo;
                }
                /*if (Input.GetKeyUp(KeyCode.LeftArrow)
                    || Input.GetKeyUp(KeyCode.UpArrow)
                    || Input.GetKeyUp(KeyCode.RightArrow)
                    || Input.GetKeyUp(KeyCode.DownArrow))
                    keyLocked = KeyCombo.None;*/
                if (Input.GetKeyDown(KeyCode.X)) {
                    if (Input.GetKey(KeyCode.LeftArrow) && keyCombos.Count == 0)
                        keyCombos.Add(KeyCombo.Left);
                    if (Input.GetKey(KeyCode.UpArrow) && keyCombos.Count == 0)
                        keyCombos.Add(KeyCombo.Up);
                    if (Input.GetKey(KeyCode.RightArrow) && keyCombos.Count == 0)
                        keyCombos.Add(KeyCombo.Right);
                    if (Input.GetKey(KeyCode.DownArrow) && keyCombos.Count == 0)
                        keyCombos.Add(KeyCombo.Down);
                    keyCombos.Add(KeyCombo.X);
                    keyComboTimer = 0;
                    keyLocked = KeyCombo.None;
                    if (nextAttack < Time.time) {
                        EvaluateCombo(keyCombos);
                        keyCombos.Clear();
                    }
                    else {
                        queuedCombos.Clear();
                        foreach (KeyCombo key in keyCombos) {
                            queuedCombos.Add(key);
                        }
                        keyCombos.Clear();
                    }
                }

                if (character.IsOnGround() 
                && character.GetState() != CharacterStates.Jump
                && character.GetState() != CharacterStates.Shift) {
                        /*if (Input.GetKeyDown(KeyCode.X)) {
                            attackComboTimer = 0;
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
                        }*/

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

                        if (Input.GetKeyDown(KeyCode.G) && character.GetInventory().GetCount("item_grenade") > 0) {
                            if(character.GetState() != CharacterStates.Throw) {
                                character.GetAnimator().Play("throw_ready");
                                character.GetInventory().ModCount("item_grenade", -1);
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
                    /*if (Input.GetKeyDown(KeyCode.X)) {
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
                    }*/
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
			if(Input.GetKeyDown(KeyCode.Delete)) {
				LevelManager.instance.DestroyMap();
			}
        }
        else {
            if (Input.GetKeyDown(KeyCode.Escape))
                PlayerPauseUI.GoBack();
        }
    }
}
