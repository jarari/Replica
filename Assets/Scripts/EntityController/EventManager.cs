using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 이벤트 시스템
 * 호출 방법
 * EventManager.RegisterEvent(이벤트명, 타입, 함수)
 * EventManager.UnregisterEvent(이벤트명)
 * 필요없는 이벤트는 제때제때 지워줄 것 
 * 예: 캐릭터의 경우 OnDestroy에서 이벤트 제거 */

public class EventFunc {
    public Delegate func;
    public Type type;
    public EventFunc(Delegate f, Type t) {
        func = f;
        type = t;
    }
}

public static class EventManager {
    public delegate void OnCharacterCreated(Character c, Vector2 pos);
    public static OnCharacterCreated Event_CharacterCreated;

    public delegate void OnCharacterDeath(Character c);
    public static OnCharacterDeath Event_CharacterDeath;

    public delegate void OnCharacterKilled(Character victim, Character attacker);
    public static OnCharacterKilled Event_CharacterKilled;

    public delegate void OnCharacterHit(Character victim, Character attacker, float damage, float stagger);
    public static OnCharacterHit Event_CharacterHit;

    public delegate void OnCharacterStagger(Character victim, Character attacker, float stagger);
    public static OnCharacterStagger Event_CharacterStagger;

    public delegate void OnWeaponEquipped(Character c, Weapon w);
    public static OnWeaponEquipped Event_WeaponEquipped;

    public delegate void OnWeaponUnequipped(Character c, Weapon w);
    public static OnWeaponUnequipped Event_WeaponUnequipped;

    public delegate void OnWeaponAttack(Character owner, Weapon w, string eventname);
    public static OnWeaponAttack Event_WeaponAttack;

    public delegate void OnWeaponFire(Character owner, Weapon w, Bullet b);
    public static OnWeaponFire Event_WeaponFire;

    public delegate void OnWeaponEvent(Character owner, Weapon w, string eventname);
    public static OnWeaponEvent Event_WeaponEvent;

    public delegate void OnItemAdded(GameObject inventoryOwner, Inventory i, Item added, int count);
    public static OnItemAdded Event_ItemAdded;

    public delegate void OnItemUse(GameObject user, Item i, int count);
    public static OnItemUse Event_ItemUse;

    public delegate void OnItemRemoved(GameObject inventoryOwner, Inventory i, Item removed, int count);
    public static OnItemRemoved Event_ItemRemoved;

    public delegate void OnAttachmentEquipped(Character c, Attachment a);
    public static OnAttachmentEquipped Event_AttachmentEquipped;

    public delegate void OnAttachmentUnequipped(Character c, Attachment a);
    public static OnAttachmentUnequipped Event_AttachmentUnequipped;

    private static Dictionary<string, EventFunc> eventFunctions = new Dictionary<string, EventFunc>();

    public static void RegisterEvent(string id, Type t, Delegate func) {
        if (eventFunctions.ContainsKey(id)) return;
        eventFunctions.Add(id, new EventFunc(func, t));
        if (t.Equals(typeof(OnCharacterCreated))) {
            Event_CharacterCreated += (OnCharacterCreated)func;
        }
        else if (t.Equals(typeof(OnCharacterDeath))) {
            Event_CharacterDeath += (OnCharacterDeath)func;
        }
        else if (t.Equals(typeof(OnCharacterKilled))) {
            Event_CharacterKilled += (OnCharacterKilled)func;
        }
        else if (t.Equals(typeof(OnCharacterHit))) {
            Event_CharacterHit += (OnCharacterHit)func;
        }
        else if (t.Equals(typeof(OnCharacterStagger))) {
            Event_CharacterStagger += (OnCharacterStagger)func;
        }
        else if (t.Equals(typeof(OnWeaponEquipped))) {
            Event_WeaponEquipped += (OnWeaponEquipped)func;
        }
        else if (t.Equals(typeof(OnWeaponUnequipped))) {
            Event_WeaponUnequipped += (OnWeaponUnequipped)func;
        }
        else if (t.Equals(typeof(OnWeaponAttack))) {
            Event_WeaponAttack += (OnWeaponAttack)func;
        }
        else if (t.Equals(typeof(OnWeaponFire))) {
            Event_WeaponFire += (OnWeaponFire)func;
        }
        else if (t.Equals(typeof(OnWeaponEvent))) {
            Event_WeaponEvent += (OnWeaponEvent)func;
        }
        else if (t.Equals(typeof(OnItemAdded))) {
            Event_ItemAdded += (OnItemAdded)func;
        }
        else if (t.Equals(typeof(OnItemUse))) {
            Event_ItemUse += (OnItemUse)func;
        }
        else if (t.Equals(typeof(OnItemRemoved))) {
            Event_ItemRemoved += (OnItemRemoved)func;
        }
        else if (t.Equals(typeof(OnAttachmentEquipped))) {
            Event_AttachmentEquipped += (OnAttachmentEquipped)func;
        }
        else if (t.Equals(typeof(OnAttachmentUnequipped))) {
            Event_AttachmentUnequipped += (OnAttachmentUnequipped)func;
        }
    }

    public static void UnregisterEvent(string id) {
        if (!eventFunctions.ContainsKey(id)) return;
        Delegate func = eventFunctions[id].func;
        Type t = eventFunctions[id].type;
        if (t.Equals(typeof(OnCharacterCreated))) {
            Event_CharacterCreated -= (OnCharacterCreated)func;
        }
        else if (t.Equals(typeof(OnCharacterDeath))) {
            Event_CharacterDeath -= (OnCharacterDeath)func;
        }
        else if (t.Equals(typeof(OnCharacterKilled))) {
            Event_CharacterKilled -= (OnCharacterKilled)func;
        }
        else if (t.Equals(typeof(OnCharacterHit))) {
            Event_CharacterHit -= (OnCharacterHit)func;
        }
        else if (t.Equals(typeof(OnCharacterStagger))) {
            Event_CharacterStagger -= (OnCharacterStagger)func;
        }
        else if (t.Equals(typeof(OnWeaponEquipped))) {
            Event_WeaponEquipped -= (OnWeaponEquipped)func;
        }
        else if (t.Equals(typeof(OnWeaponUnequipped))) {
            Event_WeaponUnequipped -= (OnWeaponUnequipped)func;
        }
        else if (t.Equals(typeof(OnWeaponAttack))) {
            Event_WeaponAttack -= (OnWeaponAttack)func;
        }
        else if (t.Equals(typeof(OnWeaponFire))) {
            Event_WeaponFire -= (OnWeaponFire)func;
        }
        else if (t.Equals(typeof(OnWeaponEvent))) {
            Event_WeaponEvent -= (OnWeaponEvent)func;
        }
        else if (t.Equals(typeof(OnItemAdded))) {
            Event_ItemAdded -= (OnItemAdded)func;
        }
        else if (t.Equals(typeof(OnItemUse))) {
            Event_ItemUse -= (OnItemUse)func;
        }
        else if (t.Equals(typeof(OnItemRemoved))) {
            Event_ItemRemoved -= (OnItemRemoved)func;
        }
        else if (t.Equals(typeof(OnAttachmentEquipped))) {
            Event_AttachmentEquipped -= (OnAttachmentEquipped)func;
        }
        else if (t.Equals(typeof(OnAttachmentUnequipped))) {
            Event_AttachmentUnequipped -= (OnAttachmentUnequipped)func;
        }
    }
}