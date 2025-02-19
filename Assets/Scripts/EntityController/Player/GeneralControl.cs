using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 플레이어 조작 */
public class GeneralControl : Controller {
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


	//이동 관련 변수
	private float sprintDoubleTabTimer = 0;
	private float sprintDoubleTabTime = 0.3f;
	protected bool waitSprintDoubleTab = false;
	protected bool canSprint = false;
	protected int sprint = 0;
	protected int walk = 0;
	protected bool sprintDoubleTabTimerToggle = false;

    private void EvaluateCombo(List<KeyCombo> kc) {
        if (nextAttack >= Time.time || character.GetState() == CharacterStates.Shift)
            return;
        if (character.GetState() != CharacterStates.Attack || currentCombo == null) {
            foreach(KeyValuePair<string, ComboData> kvp in GameDataManager.instance.BasicComboData) {
                if ((character.IsOnGround() && !kvp.Value.isJumpAttack)
                    || (!character.IsOnGround() && kvp.Value.isJumpAttack)) {
                    if (kc.Count - kvp.Value.keyCombos.Count >= 2)
                        continue;
                    int i = 0;
                    while (kvp.Value.keyCombos[0] != kc[i] && i < kc.Count - 1) {
                        i++;
                    }
                    int matchCount = 0;
                    bool matchLast = false;
                    for (int j = 0; j < kvp.Value.keyCombos.Count; j++) {
                        if (kvp.Value.keyCombos[j] == kc[i]) {
                            matchCount++;
                            i++;
                            if (i == kc.Count) {
                                matchLast = true;
                                break;
                            }
                        }
                    }
                    if (matchCount >= Math.Round(kvp.Value.keyCombos.Count * 0.8f) && matchLast) {
                        currentCombo = kvp.Value;
                        character.GetAnimator().Play(kvp.Key);
                        nextAttack = Time.time + 0.25f;
                    }
                }
            }
        }
        else {
            foreach (string nextCombo in currentCombo.nextPossibleCombos) {
                ComboData next = GameDataManager.instance.ContinuousComboData[nextCombo];
                if ((character.IsOnGround() && !next.isJumpAttack)
                    || (!character.IsOnGround() && next.isJumpAttack)) {
                    if (kc.Count - next.keyCombos.Count >= 2)
                        continue;
                    int i = 0;
                    while (next.keyCombos[0] != kc[i] && i < kc.Count - 1) {
                        i++;
                    }
                    int matchCount = 0;
                    bool matchLast = false;
                    for (int j = 0; j < next.keyCombos.Count; j++) {
                        if (next.keyCombos[j] == kc[i]) {
                            matchCount++;
                            i++;
                            if (i == kc.Count) {
                                matchLast = true;
                                break;
                            }
                        }
                    }
                    if (matchCount >= Math.Round(next.keyCombos.Count * 0.8f) && matchLast) {
                        currentCombo = next;
                        character.GetAnimator().Play(nextCombo);
                        nextAttack = Time.time + 0.25f;
                    }
                }
            }
        }
    }

    protected override void Update() {
        base.Update();

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

		//스프린트 타이머
		if(sprintDoubleTabTimer > 0) {
			sprintDoubleTabTimer = Mathf.Max(sprintDoubleTabTimer - Time.deltaTime, 0);
			waitSprintDoubleTab = true;
		}
		else {
			waitSprintDoubleTab = false;
		}
		
		if (!MenuManager.instance.IsPaused()) {
            if (character.GetUncontrollableTimeLeft() == 0 && !character.HasFlag(CharacterFlags.AIControlled)) {
				
				//이동
                if (Input.GetKey(KeyCode.RightArrow)) {
					if(sprint == 1) {
						character.Sprint(1);
					}
					else {
						if(sprintDoubleTabTimerToggle) {
							sprintDoubleTabTimer = sprintDoubleTabTime;
							sprintDoubleTabTimerToggle = false; // 타이머 연속 초기화 제한
						}

						if(canSprint && walk == 1) { // 떼고 다시 누를 때 이전에 누른 키의 방향이 같아야 함
							sprint = 1;
							waitSprintDoubleTab = false; // 스프린트 시작 시 타이머 종료 (무조건 더블탭에만 반응 삼연타시 불가)
							sprintDoubleTabTimer = 0f;
						}
						else {
							sprint = 0;
                            character.Walk(1);
							walk = 1;
						}
						canSprint = false; // 스프린트 중 방향 전환 시 스프린트 유지 제한
					}
                }
                else if (Input.GetKey(KeyCode.LeftArrow)) {
					if(sprint == -1) {
                        character.Sprint(-1);
					}
					else {
						if(sprintDoubleTabTimerToggle) {
							sprintDoubleTabTimer = sprintDoubleTabTime;
							sprintDoubleTabTimerToggle = false;
						}

						if(canSprint && walk == -1) {
							sprint = -1;
							waitSprintDoubleTab = false;
							sprintDoubleTabTimer = 0f;
						}
						else {
							sprint = 0;
                            character.Walk(-1);
							walk = -1;
						}
						canSprint = false;
					}
				}
				else {
					sprint = 0; //키 미입력시 스프린트 풀림
					if(waitSprintDoubleTab) { // 타이머 동작 중 키를 뗐을 때 다시 누르면 스프린트 가능하게 함
						canSprint = true;
					}
					else { // 타이머가 다 돼야 타이머 재생성 가능 하게 함
						canSprint = false;
						sprintDoubleTabTimerToggle = true;
					}
				}

                if (Input.GetKeyDown(KeyCode.LeftShift) && character.CanDash()) {
                    if (Input.GetKey(KeyCode.LeftArrow)) {
                        character.Dash(-1);
                    }
                    else if (Input.GetKey(KeyCode.RightArrow)) {
                        character.Dash(1);
                    }
                    else {
                        character.Dash(0);
                    }
                }

                if (character.GetState() != CharacterStates.Attack
                    && character.GetState() != CharacterStates.Throw) {
                    if (Input.GetKey(KeyCode.DownArrow)) {
                        character.Sit();
                    }

                    if (Input.GetKeyDown(KeyCode.Space)) {
                        if (Input.GetKey(KeyCode.DownArrow)) {
                            character.GoDown();
                        }
                        else {
                            character.Jump();
                        }
                    }
                }
                else if (character.GetState() == CharacterStates.Attack
                    && Input.GetKey(KeyCode.DownArrow)
                    && character.GetAnimator().GetCurrentAnimatorStateInfo(0).IsName("HG Continue"))
                    character.Sit();

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
                MenuManager.instance.ShowMenu("PauseMenu");
        }
        else {
            if (Input.GetKeyDown(KeyCode.Escape))
                MenuManager.instance.GoBack();
        }
    }

    public override void ResetAttackTimer() {
        if (character.GetLastUsedWeapon() == null)
            return;
        nextAttack = Time.time + 1f / character.GetCurrentStat(character.GetLastUsedWeapon(), WeaponStats.AttackSpeed);
    }
}
