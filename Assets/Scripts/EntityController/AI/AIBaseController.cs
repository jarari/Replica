﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//기본 AI 움직임 클래스
public class AIBaseController : Controller {
    protected float nextAttack;
    protected float nextSearch;
    protected float recognitionRadius = 2f;
    protected float minWanderDist = 0.3f;
    protected float maxWanderDist = 3f;
    protected float restingTime = 3f;
    protected float alertRange = 200f;
    protected float rangeX = 100f;
    protected float rangeY = 20f;
    protected int direction;
    protected float distance;
    protected float lastX;
    protected float restTimer;
    protected Character target;
    protected bool targetFound = false;
    protected string command = "Search";
    protected bool AIEnabled = true;
    protected string AIData;

	JDictionary aiData;
    //초기 데이터 설정
    public void Initialize(Character c, string aidata) {
        base.Initialize(c);
        nextAttack = Time.time;
        nextSearch = Time.time;
        AIData = aidata;
		aiData = GameDataManager.instance.RootData[AIData];

        recognitionRadius = aiData["RecognitionRadius"].Value<float>();
        minWanderDist = aiData["MinWanderDistance"].Value<float>();
        maxWanderDist = aiData["MaxWanderDistance"].Value<float>();
        restingTime = aiData["RestingTime"].Value<float>();
        if (aiData["AttackRangeY"])
            rangeY = aiData["AttackRangeY"].Value<float>();
        if (aiData["AttackRangeX"])
            rangeX = aiData["AttackRangeX"].Value<float>();

        direction = 1;
        distance = 0;
        restTimer = 1f;
        targetFound = false;
        AIEnabled = true;
        AdditionalData(aidata);
    }

    //Override this for other properties.
    protected virtual void AdditionalData(string aidata) {

    }

    /*
     * AI의 행동은 기본적으로 스트링 명령
     * 명령 이름 = 함수명
     *                              */
    public string GetCurrentCommand() {
        return command;
    }

    public void SetCommand(string c) {
        command = c;
    }

    public void SetAIStatus(bool stat) {
        AIEnabled = stat;
    }

    /*
     * 타겟 관련 함수
     *              */
    public void ForceTarget(Character c) {
        targetFound = true;
        target = c;
        SetCommand("Chase");
    }

    public Character GetTarget() {
        return target;
    }

    protected override void Update() {
        base.Update();
        if (!AIEnabled) return;
        Invoke(command, 0.0f);
    }

    /*
     * AI는 매 초마다 ai데이터의 정보값에 따라 다른 영역을 탐색하고 감지함.
     * 플레이어 (또는 플레이어의 아군)이 감지 범위 내에 들어와있고 AI가 해당 방향을 바라보고 있다면 추격 시작.
     * 감지 범위의 25%부터는 무조건 추격.
     * 이외의 경우에는 계속 돌아다님. */
    protected virtual void Search() {
        if (distance <= 10) {
            direction = (int)((UnityEngine.Random.Range(0, 2) - 0.5f) * 2f);
            distance = UnityEngine.Random.Range(minWanderDist, maxWanderDist);
            lastX = transform.position.x;
            restTimer = restingTime;
        }
        if (Time.time >= nextSearch) {
            nextSearch = Time.time + 1f;
            Character closestPlayer = CharacterManager.GetClosestEnemy(transform.position, character.GetTeam());
            if (closestPlayer != null) {
                if ((Vector3.Distance(closestPlayer.transform.position, character.transform.position) < recognitionRadius
                        && Mathf.Sign(closestPlayer.transform.position.x - transform.position.x) == direction
                        && !Helper.IsBlockedByMap(closestPlayer.transform.position, character.transform.position))
                        || Vector3.Distance(closestPlayer.transform.position, character.transform.position) < recognitionRadius * 0.25f) {
                    targetFound = true;
                    target = closestPlayer;
                    SetCommand("Chase");
                }
            }
        }
        distance -= Mathf.Abs(transform.position.x - lastX);
        restTimer = Mathf.Clamp(restTimer - Time.deltaTime, 0, restTimer);
        if(restTimer == 0) {
            Walk();
            if (Physics2D.OverlapBox((Vector2)transform.position + box.offset + new Vector2(box.size.x / 2f + 32 * direction, 16 - box.size.y / 2f), new Vector2(16, 5), 0, Helper.mapLayer) != null)
                direction *= -1;
        }
        else {
            character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
        }
        lastX = transform.position.x;
    }

    /* 추격 알고리즘 */
    protected virtual void Chase() {
        if(target == null) {
            targetFound = false;
            SetCommand("Search");
            character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
        }
        else {
            if(Vector3.Distance(target.transform.position, character.transform.position) >= recognitionRadius) {
                target = null;
                targetFound = false;
                SetCommand("Search");
                character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
            }
            else {
                if (character.GetState() != CharacterStates.Attack) {
                    Vector2 diff = Helper.GetClosestBoxBorder(target.transform.position, target.GetComponent<BoxCollider2D>(), transform.position) - (Vector2)transform.position;
                    if (Mathf.Abs(diff.x) <= rangeX) {
                        if (Mathf.Sign(target.transform.position.x - transform.position.x) == character.GetFacingDirection()
                            && MathF.Abs(diff.y) <= rangeY && !target.IsAI()) {
                            Attack();
                            return;
                        }
                        else if (character.GetUncontrollableTimeLeft() == 0) {
                            direction = (int)Mathf.Sign(distance);
                            if (direction == 1)
                                character.FlipFace(true);
                            else if (direction == -1)
                                character.FlipFace(false);
                        }
                        character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
                    }
                    else {
                        direction = (int)Mathf.Sign(distance);
                        character.SetFollow(target.transform.position, rangeX * 0.9f);
                        return;
                    }
                }
                else
                    character.GetAnimator().SetBool("DiscardFromAnyState", true);
            }
        }
    }

    protected virtual void Walk() {
        character.Walk(direction);
    }

    protected virtual void Attack() {
        if (Time.time < nextAttack) {
            return;
        }
        nextAttack = Time.time + 1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed);
        character.AddUncontrollableTime(Mathf.Min(1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed), 0.2f));
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Attack);
    }

    /* 슈퍼아머가 없는 AI는 타격당하면 공격 타이머가 리셋됨. */
    public override void ResetAttackTimer() {
        nextAttack = Time.time + 1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed);
    }

    /* AI가 공격당하면 주위의 아군에게도 어그로가 전달됨. */
    public virtual void OnTakeDamage(Character attacker) {
        List<Character> closeAllies = CharacterManager.GetAllies(character.GetTeam()).FindAll(c => Vector3.Distance(c.transform.position, transform.position) <= alertRange);
        foreach (Character c in closeAllies) {
            if (c.IsAI())
                ((AIBaseController)c.GetController()).ForceTarget(attacker);
        }
    }
}
