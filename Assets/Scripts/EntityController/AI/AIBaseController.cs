using System.Collections.Generic;
using UnityEngine;

//기본 AI 움직임 클래스
public class AIBaseController : BasicCharacterMovement {
    protected float nextAttack;
    protected float nextSearch;
    protected float recognitionRadius = 2f;
    protected float minWanderDist = 0.3f;
    protected float maxWanderDist = 3f;
    protected float restingTime = 3f;
    protected float alertRange = 200f;
    protected int direction;
    protected float distance;
    protected float lastX;
    protected float restTimer;
    protected Character target;
    protected bool targetFound = false;
    protected string command = "Search";
    protected bool AIEnabled = true;
    protected string AIData;

    //초기 데이터 설정
    public void Initialize(Character c, string aidata) {
        base.Initialize(c);
        nextAttack = Time.realtimeSinceStartup;
        nextSearch = Time.realtimeSinceStartup;
        AIData = aidata;
        recognitionRadius = (float)GameDataManager.instance.GetData("Data", aidata, "RecognitionRadius");
        minWanderDist = (float)GameDataManager.instance.GetData("Data", aidata, "MinWanderDistance");
        maxWanderDist = (float)GameDataManager.instance.GetData("Data", aidata, "MaxWanderDistance");
        restingTime = (float)GameDataManager.instance.GetData("Data", aidata, "RestingTime");
        direction = 1;
        distance = 0;
        restTimer = 1f;
        targetFound = false;
        AIEnabled = true;
        minDistToDash = -1f;
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
     * 감지 범위의 40%부터는 무조건 추격.
     * 이외의 경우에는 계속 돌아다님. */
    protected virtual void Search() {
        if (distance <= 0) {
            direction = (int)((Random.Range(0, 2) - 0.5f) * 2f);
            distance = Random.Range(minWanderDist, maxWanderDist);
            lastX = transform.position.x;
            restTimer = restingTime;
        }
        if (Time.realtimeSinceStartup >= nextSearch) {
            nextSearch = Time.realtimeSinceStartup + 1f;
            Character closestPlayer = CharacterManager.instance.GetClosestEnemy(transform.position, character.GetTeam());
            if (closestPlayer != null) {
                if ((Vector3.Distance(closestPlayer.transform.position, character.transform.position) < recognitionRadius
                        && Mathf.Sign(closestPlayer.transform.position.x - transform.position.x) == direction)
                        || Vector3.Distance(closestPlayer.transform.position, character.transform.position) < recognitionRadius * 0.4f) {
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
            if (Physics2D.OverlapBox((Vector2)transform.position + new Vector2(32 * dir, 0), new Vector2(16, 10), 0, mapLayer) != null
                            && Physics2D.OverlapBox((Vector2)transform.position + new Vector2(32 * dir, maxJump), new Vector2(16, 5), 0, mapLayer) != null)
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
        }
        else {
            if(character.GetState() != CharacterStates.Attack) {
                distance = target.transform.position.x - transform.position.x;
                if (Mathf.Abs(distance) <= character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.Range) * 0.9f) {
                    if (Mathf.Sign(target.transform.position.x - transform.position.x) == character.GetFacingDirection()
                        && Helper.GetClosestBoxBorder(target.transform.position, target.GetComponent<BoxCollider2D>(), transform.position).y - transform.position.y <= 100) {
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
                }
                else {
                    direction = (int)Mathf.Sign(distance);
                    Follow(target.transform.position, character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.Range) * 0.9f);
                    return;
                }
                character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
            }
            else
                character.GetAnimator().SetBool("DiscardFromAnyState", true);
        }
    }

    protected virtual void Walk() {
        Walk(direction);
    }

    protected virtual void Attack() {
        if (Time.realtimeSinceStartup < nextAttack) return;
        nextAttack = Time.realtimeSinceStartup + 1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed);
        character.AddUncontrollableTime(Mathf.Min(1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed), 0.2f));
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Attack);
    }

    /* 슈퍼아머가 없는 AI는 타격당하면 공격 타이머가 리셋됨. */
    protected override void OnHitEvent(int invincible) {
        base.OnHitEvent(invincible);
        nextAttack = Time.realtimeSinceStartup + 1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed);
    }

    /* AI가 공격당하면 주위의 아군에게도 어그로가 전달됨. */
    public virtual void OnTakeDamage(Character attacker) {
        List<Character> closeAllies = CharacterManager.instance.GetAllies(character.GetTeam()).FindAll(c => Vector3.Distance(c.transform.position, transform.position) <= alertRange);
        foreach (Character c in closeAllies) {
            if (c.IsAI())
                ((AIBaseController)c.GetController()).ForceTarget(attacker);
        }
    }
}
