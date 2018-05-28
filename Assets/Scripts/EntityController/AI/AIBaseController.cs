using System.Collections.Generic;
using UnityEngine;

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

    public string GetCurrentCommand() {
        return command;
    }

    public void SetCommand(string c) {
        command = c;
    }

    public void SetAIStatus(bool stat) {
        AIEnabled = stat;
    }

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
                if (Vector3.Distance(closestPlayer.transform.position, character.transform.position) < recognitionRadius
                        && Mathf.Sign(closestPlayer.transform.position.x - transform.position.x) == direction) {
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

    protected virtual void Chase() {
        if(target == null) {
            targetFound = false;
            SetCommand("Search");
        }
        else {
            distance = target.transform.position.x - transform.position.x;
            if(Mathf.Abs(distance) <= character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.Range) * 0.9f) {
                character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
                if (Mathf.Sign(target.transform.position.x - transform.position.x) == (System.Convert.ToSingle(character.IsFacingRight()) - 0.5f) * 2f) {
                    Attack();
                    return;
                }
                else if(character.GetUncontrollableTimeLeft() == 0){
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

    public virtual void OnTakeDamage(Character attacker) {
        List<Character> closeAllies = CharacterManager.instance.GetAllies(character.GetTeam()).FindAll(c => Vector3.Distance(c.transform.position, transform.position) <= alertRange);
        foreach (Character c in closeAllies) {
            if (c.IsAI())
                ((AIBaseController)c.GetController()).ForceTarget(attacker);
        }
    }
}
