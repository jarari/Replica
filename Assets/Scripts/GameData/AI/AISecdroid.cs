using System.Collections;
using UnityEngine;

public class AISecdroid : AIBaseController {
    protected override void AdditionalData(string aidata) {
        recognitionRadius = 9999f;
        AIEnabled = false;
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    private void OnSpawnEvent() {
        AIEnabled = true;
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
        character.SetFlag(CharacterFlags.StaggerImmunity);
        SetCommand("Sit");
    }

    private void Sit() {
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Sit);
        character.SetState(CharacterStates.Sit);
    }

    protected override void Attack() {
        if (Time.time < nextAttack) {
            character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
            return;
        }
        nextAttack = Time.time + 1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed);
        character.AddUncontrollableTime(Mathf.Min(1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed), 0.2f));
        character.GetAnimator().SetInteger("AttackType", Random.Range(1, 3));
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Attack);
    }

    public void ForceAttack(int attackType) {
        nextAttack = Time.time + 1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed);
        character.AddUncontrollableTime(Mathf.Min(1f / character.GetCurrentStat(character.GetWeapon(WeaponTypes.AI), WeaponStats.AttackSpeed), 0.2f));
        character.GetAnimator().SetInteger("AttackType", attackType);
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Attack);
        StartCoroutine(ForcedAttack());
    }

    IEnumerator ForcedAttack() {
        yield return new WaitWhile(() => character.GetState() != CharacterStates.Attack);
        character.GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
        while (character.GetState() == CharacterStates.Attack) {
            character.GetAnimator().SetBool("DiscardFromAnyState", true);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
