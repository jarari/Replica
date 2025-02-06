using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 이벤트 시스템
 * 호출 방법
 * EventManager.RegisterEvent(이벤트명, delegate 함수)
 * EventManager.UnregisterEvent(이벤트명)
 * 필요없는 이벤트는 제때제때 지워줄 것 
 * 예: 캐릭터의 경우 OnDestroy에서 이벤트 제거 
 * 실제 사용 예시는 PlayerHUD 클래스 31번줄 참조
 */

public class EventFunc {
	public Type Type { get; private set; }
	public Delegate Func { get; private set; }

    public EventFunc(Delegate f, Type t) {
		this.Func = f;
		this.Type = t;
    }
}

public static class EventManager {

	#region Delegates
	public delegate void CharacterCreated(Character character, Vector2 pos);
	public delegate void CharacterDeath(Character character);
	public delegate void CharacterKilled(Character victim, Character attacker);
	public delegate void CharacterHealthChanged(Character character, float changed);
	public delegate void CharacterHit(Character victim, Character attacker, float damage, float stagger);
	public delegate void CharacterStagger(Character victim, Character attacker, float stagger);

	public delegate void WeaponEquipped(Character character, Weapon weapon);
	public delegate void WeaponUnequipped(Character character, Weapon weapon);
	public delegate void WeaponAttack(Character owner, Weapon weapon, string eventname);
	public delegate void WeaponFire(Character owner, Weapon weapon, string bullet);
	public delegate void WeaponEvent(Character owner, Weapon weapon, string eventname);

	public delegate void ItemAdded(GameObject inventoryOwner, Inventory inventory, Item added, int count);
	public delegate void ItemUsed(GameObject user, Item used, int count);
	public delegate void ItemRemoved(GameObject inventoryOwner, Inventory inventory, Item removed, int count);

	public delegate void AttachmentEquipped(Character character, Attachment attachment);
	public delegate void AttachmentUnequipped(Character character, Attachment attachment);

    public delegate void MapCreated();
    public delegate void MapDestroyed();
	#endregion

	#region Events
	private static CharacterCreated Event_CharacterCreated;
	private static CharacterDeath Event_CharacterDeath;
	private static CharacterKilled Event_CharacterKilled;
	private static CharacterHealthChanged Event_CharacterHealthChanged;
	private static CharacterHit Event_CharacterHit;
	private static CharacterStagger Event_CharacterStagger;

	private static WeaponEquipped Event_WeaponEquipped;
	private static WeaponUnequipped Event_WeaponUnequipped;
	private static WeaponAttack Event_WeaponAttack;
	private static WeaponFire Event_WeaponFire;
	private static WeaponEvent Event_WeaponEvent;

	private static ItemAdded Event_ItemAdded;
	private static ItemUsed Event_ItemUse;
	private static ItemRemoved Event_ItemRemoved;

	private static AttachmentEquipped Event_AttachmentEquipped;
	private static AttachmentUnequipped Event_AttachmentUnequipped;

    private static MapCreated Event_MapCreated;
    private static MapDestroyed Event_MapDestroyed;
	#endregion

	private static Dictionary<string, EventFunc> eventFunctions = new Dictionary<string, EventFunc>();

    public static void RegisterEvent(string id, Delegate func) {
        if (eventFunctions.ContainsKey(id)) {
			Debug.LogWarning("Event " + id + " already exists!");
			return;
		}

		Type t = func.GetType();
        if (t.Equals(typeof(CharacterCreated))) {
			Event_CharacterCreated += (CharacterCreated) func;
        }
        else if (t.Equals(typeof(CharacterDeath))) {
			Event_CharacterDeath += (CharacterDeath) func;
        }
        else if (t.Equals(typeof(CharacterKilled))) {
			Event_CharacterKilled += (CharacterKilled) func;
        }
        else if (t.Equals(typeof(CharacterHealthChanged))) {
			Event_CharacterHealthChanged += (CharacterHealthChanged) func;
        }
        else if (t.Equals(typeof(CharacterHit))) {
			Event_CharacterHit += (CharacterHit) func;
        }
        else if (t.Equals(typeof(CharacterStagger))) {
			Event_CharacterStagger += (CharacterStagger) func;
        }
        else if (t.Equals(typeof(WeaponEquipped))) {
			Event_WeaponEquipped += (WeaponEquipped) func;
        }
        else if (t.Equals(typeof(WeaponUnequipped))) {
			Event_WeaponUnequipped += (WeaponUnequipped) func;
        }
        else if (t.Equals(typeof(WeaponAttack))) {
			Event_WeaponAttack += (WeaponAttack) func;
        }
        else if (t.Equals(typeof(WeaponFire))) {
			Event_WeaponFire += (WeaponFire) func;
        }
        else if (t.Equals(typeof(WeaponEvent))) {
			Event_WeaponEvent += (WeaponEvent) func;
        }
        else if (t.Equals(typeof(ItemAdded))) {
			Event_ItemAdded += (ItemAdded) func;
        }
        else if (t.Equals(typeof(ItemUsed))) {
			Event_ItemUse += (ItemUsed) func;
        }
        else if (t.Equals(typeof(ItemRemoved))) {
			Event_ItemRemoved += (ItemRemoved) func;
        }
        else if (t.Equals(typeof(AttachmentEquipped))) {
			Event_AttachmentEquipped += (AttachmentEquipped) func;
        }
        else if (t.Equals(typeof(AttachmentUnequipped))) {
			Event_AttachmentUnequipped += (AttachmentUnequipped) func;
		}
        else if (t.Equals(typeof(MapCreated))) {
            Event_MapCreated += (MapCreated)func;
        }
        else if (t.Equals(typeof(MapDestroyed))) {
            Event_MapDestroyed += (MapDestroyed)func;
        }

        eventFunctions.Add(id, new EventFunc(func, t));
	}
    public static void UnregisterEvent(string id) {
        if(!eventFunctions.ContainsKey(id)) {
			Debug.LogWarning("Event " + id + " doesn't exists!");
			return;
		}

        Type t = eventFunctions[id].Type;
		Delegate func = eventFunctions[id].Func;
		if (t.Equals(typeof(CharacterCreated))) {
			Event_CharacterCreated -= (CharacterCreated) func;
        }
        else if (t.Equals(typeof(CharacterDeath))) {
			Event_CharacterDeath -= (CharacterDeath) func;
        }
        else if (t.Equals(typeof(CharacterKilled))) {
			Event_CharacterKilled -= (CharacterKilled) func;
        }
        else if (t.Equals(typeof(CharacterHealthChanged))) {
			Event_CharacterHealthChanged -= (CharacterHealthChanged) func;
        }
        else if (t.Equals(typeof(CharacterHit))) {
			Event_CharacterHit -= (CharacterHit) func;
        }
        else if (t.Equals(typeof(CharacterStagger))) {
			Event_CharacterStagger -= (CharacterStagger) func;
        }
        else if (t.Equals(typeof(WeaponEquipped))) {
			Event_WeaponEquipped -= (WeaponEquipped) func;
        }
        else if (t.Equals(typeof(WeaponUnequipped))) {
			Event_WeaponUnequipped -= (WeaponUnequipped) func;
        }
        else if (t.Equals(typeof(WeaponAttack))) {
			Event_WeaponAttack -= (WeaponAttack) func;
        }
        else if (t.Equals(typeof(WeaponFire))) {
			Event_WeaponFire -= (WeaponFire) func;
        }
        else if (t.Equals(typeof(WeaponEvent))) {
			Event_WeaponEvent -= (WeaponEvent) func;
        }
        else if (t.Equals(typeof(ItemAdded))) {
			Event_ItemAdded -= (ItemAdded) func;
        }
        else if (t.Equals(typeof(ItemUsed))) {
			Event_ItemUse -= (ItemUsed) func;
        }
        else if (t.Equals(typeof(ItemRemoved))) {
			Event_ItemRemoved -= (ItemRemoved) func;
        }
        else if (t.Equals(typeof(AttachmentEquipped))) {
			Event_AttachmentEquipped -= (AttachmentEquipped) func;
        }
        else if (t.Equals(typeof(AttachmentUnequipped))) {
			Event_AttachmentUnequipped -= (AttachmentUnequipped) func;
        }
        else if (t.Equals(typeof(MapCreated))) {
            Event_MapCreated -= (MapCreated)func;
        }
        else if (t.Equals(typeof(MapDestroyed))) {
            Event_MapDestroyed -= (MapDestroyed)func;
        }

        eventFunctions.Remove(id);
	}
    public static void UnregisterAll() {
		var keys = eventFunctions.Keys.ToList();
		foreach (string id in keys) {
            UnregisterEvent(id);
        }
    }

	public static void OnCharacterCreated(Character character, Vector2 pos) {
		if(Event_CharacterCreated != null)
			Event_CharacterCreated(character, pos);
	}
	public static void OnCharacterDeath(Character character) {
		if(Event_CharacterDeath != null)
			Event_CharacterDeath(character);
	}
	public static void OnCharacterKilled(Character victim, Character attacker) {
		if(Event_CharacterKilled != null)
			Event_CharacterKilled(victim, attacker);
	}
	public static void OnCharacterHealthChanged(Character character, float changed) {
		if(Event_CharacterHealthChanged != null)
			Event_CharacterHealthChanged(character, changed);
	}
	public static void OnCharacterHit(Character victim, Character attacker, float damage, float stagger) {
		if(Event_CharacterHit != null)
			Event_CharacterHit(victim, attacker, damage, stagger);
	}
	public static void OnCharacterStagger(Character victim, Character attacker, float stagger) {
		if(Event_CharacterStagger != null)
			Event_CharacterStagger(victim, attacker, stagger);
	}

	public static void OnWeaponEquipped(Character character, Weapon weapon) {
		if(Event_WeaponEquipped != null)
			Event_WeaponEquipped(character, weapon);
	}
	public static void OnWeaponUnEquipped(Character character, Weapon weapon) {
		if(Event_WeaponUnequipped != null)
			Event_WeaponUnequipped(character, weapon);
	}
	public static void OnWeaponAttack(Character owner, Weapon weapon, string eventname) {
		if(Event_WeaponAttack != null)
			Event_WeaponAttack(owner, weapon, eventname);
	}
	public static void OnWeaponFire(Character owner, Weapon weapon, string bullet) {
		if(Event_WeaponFire != null)
			Event_WeaponFire(owner, weapon, bullet);
	}
	public static void OnWeaponEvent(Character owner, Weapon weapon, string eventname) {
		if(Event_WeaponEvent != null)
			Event_WeaponEvent(owner, weapon, eventname);
	}

	public static void OnItemAdded(GameObject inventoryOwner, Inventory inventory, Item added, int count) {
		if(Event_ItemAdded != null)
			Event_ItemAdded(inventoryOwner, inventory, added, count);
	}
	public static void OnItemUsed(GameObject user, Item used, int count) {
		if(Event_ItemUse != null)
			Event_ItemUse(user, used, count);
	}
	public static void OnItemRemoved(GameObject inventoryOwner, Inventory inventory, Item removed, int count) {
		if(Event_ItemRemoved != null)
			Event_ItemRemoved(inventoryOwner, inventory, removed, count);
	}

	public static void OnAttachmentEquipped(Character character, Attachment attachment) {
		if(Event_AttachmentEquipped != null)
			Event_AttachmentEquipped(character, attachment);
	}
	public static void OnAttachmentUnequipped(Character character, Attachment attachment) {
		if(Event_AttachmentUnequipped != null)
			Event_AttachmentUnequipped(character, attachment);
	}

    public static void OnMapCreated() {
        if (Event_MapCreated != null)
            Event_MapCreated();
    }

    public static void OnMapDestroyed() {
        if (Event_MapDestroyed != null)
            Event_MapDestroyed();
    }
}