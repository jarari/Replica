using UnityEngine;

public class AISecdroid : AIBaseController {
    protected override void AdditionalData(string aidata) {
        AIEnabled = false;
        character.GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    private void OnSpawnEvent() {
        AIEnabled = true;
        character.GetAnimator().SetBool("DiscardFromAnyState", false);
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
}
