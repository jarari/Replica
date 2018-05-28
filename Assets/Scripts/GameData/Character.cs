using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterStats {
    Health,
    MoveSpeed,
    JumpPower,
    MeleeArmor,
    RangeArmor,
    SuperArmor,
    GrenadeFullCharge,
    GrenadeThrowPower,
    EndOfEnums
}

public enum CharacterStates {
    Idle,
    Walk,
    Sprint,
    Shift,
    Jump,
    Attack,
    Sit,
    Throw,
    Uncontrollable,
    None
}

[Flags] public enum CharacterFlags {
    Invincible = 1,
    AIControlled = 2,
    Boss = 4
}

public enum CharacterTypes {
    Player,
    AI,
    Boss
}

public class WeaponBuff {
    private Dictionary<WeaponStats, float> buffdata;
    private WeaponTypes weaponType;
    public WeaponBuff(Dictionary<WeaponStats, float> _data, WeaponTypes _type) {
        buffdata = _data;
        weaponType = _type;
    }

    public Dictionary<WeaponStats, float> GetBuffData() {
        return buffdata;
    }

    public WeaponTypes GetTargetType() {
        return weaponType;
    }
}

public abstract class Character : ObjectBase {
    protected Teams team;
    protected CharacterFlags flag;
    protected BasicCharacterMovement basecontroller;
    protected Dictionary<WeaponTypes, Weapon> weapons = new Dictionary<WeaponTypes, Weapon>();

    protected float uncontrollableTimer = 0.0f;
    protected bool forceUncontrollable = false;
    protected bool uncontrollable = false;

    protected float[] stats = new float[(int)CharacterStats.EndOfEnums];
    protected Dictionary<WeaponTypes, float[]> weaponstats = new Dictionary<WeaponTypes, float[]>();

    protected int hasSubSprite = 0;

    protected CharacterStates state = CharacterStates.Idle;

    protected Dictionary<string, Dictionary<CharacterStats, float>> buffs_plus = new Dictionary<string, Dictionary<CharacterStats, float>>();
    protected Dictionary<string, Dictionary<CharacterStats, float>> buffs_mul = new Dictionary<string, Dictionary<CharacterStats, float>>();
    protected Dictionary<string, float> bufftimer = new Dictionary<string, float>();

    protected Dictionary<string, WeaponBuff> weaponbuffs_plus = new Dictionary<string, WeaponBuff>();
    protected Dictionary<string, WeaponBuff> weaponbuffs_mul = new Dictionary<string, WeaponBuff>();
    protected Dictionary<string, float> weaponbufftimer = new Dictionary<string, float>();

    protected Dictionary<int, DOT> dotlist = new Dictionary<int, DOT>();
    protected List<DOT> dotlist_noid = new List<DOT>();

    void Update() {
        uncontrollableTimer = Mathf.Clamp(uncontrollableTimer - Time.deltaTime, 0, uncontrollableTimer);
        if (uncontrollableTimer > 0 || forceUncontrollable)
            uncontrollable = true;
        else
            uncontrollable = false;

        List<string> keys = new List<string>(bufftimer.Keys);
        foreach (string key in keys) {
            if(bufftimer[key] != -1) {
                float time = Mathf.Clamp(bufftimer[key] - Time.deltaTime, 0, bufftimer[key]);
                if (time == 0) {
                    RemoveBuff(key);
                    bufftimer.Remove(key);
                }
                else {
                    bufftimer[key] = time;
                }
            }
        }

        List<string> wepkeys = new List<string>(weaponbufftimer.Keys);
        foreach (string wepkey in wepkeys) {
            if (weaponbufftimer[wepkey] != -1) {
                float time = Mathf.Clamp(weaponbufftimer[wepkey] - Time.deltaTime, 0, weaponbufftimer[wepkey]);
                if (time == 0) {
                    RemoveWeaponBuff(wepkey);
                    weaponbufftimer.Remove(wepkey);
                }
                else {
                    weaponbufftimer[wepkey] = time;
                }
            }
        }

        List<int> dotkeys = new List<int>(dotlist.Keys);
        foreach (int key in dotkeys) {
            DOT dot = dotlist[key];
            if (dot.GetDuration() <= 0) {
                dotlist.Remove(key);
            }
            else {
                if (dot.ShouldDoDamage()) {
                    dot.DoDamage();
                }
            }
        }

        for (int i = dotlist_noid.Count - 1; i > -1; i--) {
            DOT dot = dotlist_noid[i];
            if (dot.GetDuration() <= 0) {
                dotlist_noid.RemoveAt(i);
            }
            else {
                if (dot.ShouldDoDamage()) {
                    dot.DoDamage();
                }
            }
        }
    }    

    public override void Initialize(string classname) {
        base.Initialize(classname);

        for (int i = 0; i < (int)CharacterStats.EndOfEnums; i++)
            stats[i] = GetBaseStat((CharacterStats)i);

        if (GameDataManager.instance.GetData("Data", className, "Sprites", "hasSub") != null)
            hasSubSprite = Convert.ToInt32(GameDataManager.instance.GetData("Data", className, "Sprites", "hasSub"));
    }

    public void AddBuff(string name, Dictionary<CharacterStats, float> data, bool isAdd, float time = -1) {
        if (isAdd && !buffs_plus.ContainsKey(name)) {
            buffs_plus.Add(name, data);
        }
        else if (!isAdd && !buffs_mul.ContainsKey(name)) {
            buffs_mul.Add(name, data);
        }
        if (time != -1 && !bufftimer.ContainsKey(name))
            bufftimer.Add(name, time);
        else
            bufftimer[name] = time;
    }

    public void RemoveBuff(string name) {
        if (buffs_plus.ContainsKey(name)) {
            buffs_plus.Remove(name);
        }
        if (buffs_mul.ContainsKey(name)) {
            buffs_mul.Remove(name);
        }
    }
    public float GetBuffedStat(float original, CharacterStats stat) {
        float plus = original;
        foreach (KeyValuePair<string, Dictionary<CharacterStats, float>> buffs in buffs_plus) {
            foreach (KeyValuePair<CharacterStats, float> data in buffs.Value) {
                if (data.Key == stat)
                    plus += data.Value;
            }
        }
        float mul = 1;
        foreach (KeyValuePair<string, Dictionary<CharacterStats, float>> buffs in buffs_mul) {
            foreach (KeyValuePair<CharacterStats, float> data in buffs.Value) {
                if (data.Key == stat)
                    mul += data.Value;
            }
        }
        return plus * mul;
    }

    public float GetUnbuffedStat(float val, CharacterStats stat) {
        float plus = 0;
        foreach (KeyValuePair<string, Dictionary<CharacterStats, float>> buffs in buffs_plus) {
            foreach (KeyValuePair<CharacterStats, float> data in buffs.Value) {
                if (data.Key == stat)
                    plus += data.Value;
            }
        }
        float mul = 1;
        foreach (KeyValuePair<string, Dictionary<CharacterStats, float>> buffs in buffs_mul) {
            foreach (KeyValuePair<CharacterStats, float> data in buffs.Value) {
                if (data.Key == stat)
                    mul += data.Value;
            }
        }
        return val / mul - plus;
    }

    public void AddWeaponBuff(string name, WeaponBuff data, bool isAdd, float time = -1) {
        if (isAdd && !weaponbuffs_plus.ContainsKey(name)) {
            weaponbuffs_plus.Add(name, data);
        }
        else if (!isAdd && !weaponbuffs_mul.ContainsKey(name)) {
            weaponbuffs_mul.Add(name, data);
        }
        if (time != -1 && !weaponbufftimer.ContainsKey(name))
            weaponbufftimer.Add(name, time);
        else
            weaponbufftimer[name] = time;
    }

    public void RemoveWeaponBuff(string name) {
        if (weaponbuffs_plus.ContainsKey(name)) {
            weaponbuffs_plus.Remove(name);
        }
        if (weaponbuffs_mul.ContainsKey(name)) {
            weaponbuffs_mul.Remove(name);
        }
    }
    public float GetBuffedWeaponStat(float original, WeaponTypes type, WeaponStats stat) {
        float plus = original;
        foreach (KeyValuePair<string, WeaponBuff> buffs in weaponbuffs_plus) {
            if (buffs.Value.GetTargetType() == type) {
                foreach (KeyValuePair<WeaponStats, float> data in buffs.Value.GetBuffData()) {
                    if (data.Key == stat)
                        plus += data.Value;
                }
            }
        }
        float mul = 1;
        foreach (KeyValuePair<string, WeaponBuff> buffs in weaponbuffs_mul) {
            if (buffs.Value.GetTargetType() == type) {
                foreach (KeyValuePair<WeaponStats, float> data in buffs.Value.GetBuffData()) {
                    if (data.Key == stat)
                        mul += data.Value;
                }
            }
        }
        return plus * mul;
    }

    public float GetUnbuffedWeaponStat(float val, WeaponTypes type, WeaponStats stat) {
        float plus = 0;
        foreach (KeyValuePair<string, WeaponBuff> buffs in weaponbuffs_plus) {
            if (buffs.Value.GetTargetType() == type) {
                foreach (KeyValuePair<WeaponStats, float> data in buffs.Value.GetBuffData()) {
                    if (data.Key == stat)
                        plus += data.Value;
                }
            }
        }
        float mul = 1;
        foreach (KeyValuePair<string, WeaponBuff> buffs in weaponbuffs_mul) {
            if (buffs.Value.GetTargetType() == type) {
                foreach (KeyValuePair<WeaponStats, float> data in buffs.Value.GetBuffData()) {
                    if (data.Key == stat)
                        mul += data.Value;
                }
            }
        }
        return val / mul - plus;
    }

    public void AddDOT(int id, float duration, float tickdamage, float tickdelay, Character attacker) {
        if (dotlist.ContainsKey(id))
            dotlist[id].SetDuration(duration);
        else {
            DOT dot = new DOT(duration, tickdamage, tickdelay, attacker, this);
            dotlist.Add(id, dot);
        }
    }

    public void AddDOT(float duration, float tickdamage, float tickdelay, Character attacker) {
        DOT dot = new DOT(duration, tickdamage, tickdelay, attacker, this);
        dotlist_noid.Add(dot);
    }

    public void RemoveDOT(int id) {
        if (dotlist.ContainsKey(id))
            dotlist.Remove(id);
    }

    public CharacterFlags GetFlag() {
        return flag;
    }

    public Teams GetTeam() {
        return team;
    }

    public void SetTeam(Teams t) {
        team = t;
    }

    public string GetName() {
        return (string)GameDataManager.instance.GetData("Data", className, "Name");
    }

    public int HasSubSprite() {
        return hasSubSprite;
    }

    public void SetBaseStat(CharacterStats stat, float val) {
        stats[(int)stat] = val;
    }

    public void ModStat(CharacterStats stat, float val) {
        stats[(int)stat] = Mathf.Clamp(GetUnbuffedStat(GetBuffedStat(GetBaseStat(stat), stat) + val, stat), 0, GetMaxStat(stat));
    }

    public void SetCurrentStat(CharacterStats stat, float val) {
        ModStat(stat, val - GetCurrentStat(stat));
    }

    public float GetBaseStat(CharacterStats stat) {
        return GameDataManager.instance.GetCharacterStat(className, stat);
    }

    public float GetMaxStat(CharacterStats stat) {
        return GetBuffedStat(GetBaseStat(stat), stat);
    }

    public float GetCurrentStat(CharacterStats stat) {
        return GetBuffedStat(stats[(int)stat], stat);
    }

    public void SetBaseStat(Weapon wep, WeaponStats stat, float val) {
        if (!weaponstats.ContainsKey(wep.GetWeaponType())) {
            weaponstats.Add(wep.GetWeaponType(), new float[(int)WeaponStats.EndOfEnums]);
        }
        weaponstats[wep.GetWeaponType()][(int)stat] = val;
    }

    public void ModStat(Weapon wep, WeaponStats stat, float val) {
        if (!weaponstats.ContainsKey(wep.GetWeaponType())) {
            weaponstats.Add(wep.GetWeaponType(), new float[(int)WeaponStats.EndOfEnums]);
        }
        weaponstats[wep.GetWeaponType()][(int)stat] = Mathf.Clamp(GetUnbuffedWeaponStat(GetBuffedWeaponStat(GetBaseStat(wep, stat), wep.GetWeaponType(), stat) + val, wep.GetWeaponType(), stat), 0, GetMaxStat(wep, stat));
    }

    public float GetBaseStat(Weapon wep, WeaponStats stat) {
        return GameDataManager.instance.GetWeaponStat(wep.GetClass(), stat);
    }

    public float GetMaxStat(Weapon wep, WeaponStats stat) {
        return GetBuffedWeaponStat(GetBaseStat(wep, stat), wep.GetWeaponType(), stat);
    }

    public float GetCurrentStat(Weapon wep, WeaponStats stat) {
        if (!weaponstats.ContainsKey(wep.GetWeaponType()))
            return 0;
        return GetBuffedWeaponStat(weaponstats[wep.GetWeaponType()][(int)stat], wep.GetWeaponType(), stat);
    }

    public void AddForce(Vector2 force) {
        GetComponentInParent<MoveObject>().AddForce(force);
    }

    public float GetUncontrollableTimeLeft() {
        return uncontrollableTimer + Convert.ToSingle(forceUncontrollable);
    }

    public void AddUncontrollableTime(float time) {
        uncontrollableTimer += time;
    }

    public void SetUncontrollable(bool b) {
        forceUncontrollable = b;
    }

    public void SetController(BasicCharacterMovement controller) {
        basecontroller = controller;
    }

    public bool IsAI() {
        return (flag & CharacterFlags.AIControlled) != 0;
    }

    public bool IsBoss() {
        return (flag & CharacterFlags.Boss) != 0;
    }

    public bool IsInvincible() {
        return (flag & CharacterFlags.Invincible) != 0;
    }

    public void SetFlag(CharacterFlags f) {
        flag |= f;
    }

    public void RemoveFlag(CharacterFlags f) {
        flag &= ~f;
    }

    public CharacterStates GetState() {
        return state;
    }

    public void SetState(CharacterStates st) {
        state = st;
    }

    public void GiveWeapon(string classname) {
        GameObject gun_obj = (GameObject)Instantiate(Resources.Load("Prefab/Weapon"), transform.position, new Quaternion());
        string script = (string)GameDataManager.instance.GetData("Data", classname, "ScriptClass");
        if (script == null || script.Length == 0)
            script = "Weapon";
        Weapon wep = (Weapon)gun_obj.AddComponent(Type.GetType(script));
        wep.SetOwner(this);
        wep.Initialize(classname);
        if (weapons.ContainsKey(wep.GetWeaponType())) {
            Destroy(weapons[wep.GetWeaponType()].gameObject);
            weapons.Remove(wep.GetWeaponType());
        }
        weapons.Add(wep.GetWeaponType(), wep);
        Vector3 lscale = gun_obj.transform.localScale;
        lscale.x = gun_obj.transform.localScale.x * Mathf.Sign(transform.localScale.x);
        gun_obj.transform.localScale = lscale;
        gun_obj.transform.SetParent(transform);
    }

    public void RemoveWeapon(WeaponTypes type) {
        if (weapons.ContainsKey(type)) {
            Destroy(weapons[type].gameObject);
            weapons.Remove(type);
        }
    }

    public Weapon GetWeapon(WeaponTypes type) {
        return weapons[type];
    }

    public BasicCharacterMovement GetController() {
        return basecontroller;
    }

    public void DoDamage(Character attacker, float damage, float stagger) {
        if (damage == 0 || (flag & CharacterFlags.Invincible) == CharacterFlags.Invincible) return;
        if (IsAI())
            ((AIBaseController)basecontroller).OnTakeDamage(attacker);
        if (!IsBoss()) {
            SetUncontrollable(true);
            StartCoroutine(Stagger(stagger));
        }
        OnHealthChanged();
        ModStat(CharacterStats.Health, -damage);
        if (GetCurrentStat(CharacterStats.Health) <= 0)
            OnDeath();
    }

    IEnumerator Stagger(float time) {
        yield return new WaitForSeconds(time);
        SetUncontrollable(false);
    }

    public void Kill() {
        DoDamage(this, GetCurrentStat(CharacterStats.Health), 0);
    }

    public virtual void OnHealthChanged() {
    }

    public virtual void OnDeath() {
        if (transform == null)
            return;
    }

    private void OnDestroy() {
        CharacterManager.instance.OnCharacterDead(this);
    }    
}
