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

public abstract class Character : ObjectBase {
    private Teams team;
    private CharacterFlags flag;
    private BasicCharacterMovement basecontroller;
    private Weapon weapon;

    private float uncontrollableTimer = 0.0f;
    private bool forceUncontrollable = false;
    private bool uncontrollable = false;

    private float[] stats = new float[(int)CharacterStats.EndOfEnums];
    private float[] weaponstats = new float[(int)WeaponStats.EndOfEnums];

    private int hasSubSprite = 0;

    private CharacterStates state = CharacterStates.Idle;

    Dictionary<string, Dictionary<CharacterStats, float>> buffs_plus = new Dictionary<string, Dictionary<CharacterStats, float>>();
    Dictionary<string, Dictionary<CharacterStats, float>> buffs_mul = new Dictionary<string, Dictionary<CharacterStats, float>>();
    Dictionary<string, float> bufftimer = new Dictionary<string, float>();

    Dictionary<string, Dictionary<WeaponStats, float>> weaponbuffs_plus = new Dictionary<string, Dictionary<WeaponStats, float>>();
    Dictionary<string, Dictionary<WeaponStats, float>> weaponbuffs_mul = new Dictionary<string, Dictionary<WeaponStats, float>>();
    Dictionary<string, float> weaponbufftimer = new Dictionary<string, float>();

    Dictionary<int, DOT> dotlist = new Dictionary<int, DOT>();
    List<DOT> dotlist_noid = new List<DOT>();

    void Update() {
        uncontrollableTimer = Mathf.Clamp(uncontrollableTimer - Time.deltaTime, 0, uncontrollableTimer);
        if (uncontrollableTimer > 0 || forceUncontrollable)
            uncontrollable = true;
        else
            uncontrollable = false;

        List<string> keys = new List<string>(bufftimer.Keys);
        foreach (string key in keys) {
            float time = Mathf.Clamp(bufftimer[key] - Time.deltaTime, 0, bufftimer[key]);
            if (time == 0) {
                RemoveBuff(key);
                bufftimer.Remove(key);
            }
            else {
                bufftimer[key] = time;
            }
        }

        List<string> wepkeys = new List<string>(weaponbufftimer.Keys);
        foreach (string wepkey in wepkeys) {
            float time = Mathf.Clamp(weaponbufftimer[wepkey] - Time.deltaTime, 0, weaponbufftimer[wepkey]);
            if (time == 0) {
                RemoveWeaponBuff(wepkey);
                weaponbufftimer.Remove(wepkey);
            }
            else {
                weaponbufftimer[wepkey] = time;
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

    public void AddWeaponBuff(string name, Dictionary<WeaponStats, float> data, bool isAdd, float time = -1) {
        if (isAdd && !buffs_plus.ContainsKey(name)) {
            weaponbuffs_plus.Add(name, data);
        }
        else if (!isAdd && !buffs_mul.ContainsKey(name)) {
            weaponbuffs_mul.Add(name, data);
        }
        if (time != -1 && !bufftimer.ContainsKey(name))
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
    public float GetBuffedWeaponStat(float original, WeaponStats stat) {
        float plus = original;
        foreach (KeyValuePair<string, Dictionary<WeaponStats, float>> buffs in weaponbuffs_plus) {
            foreach (KeyValuePair<WeaponStats, float> data in buffs.Value) {
                if (data.Key == stat)
                    plus += data.Value;
            }
        }
        float mul = 1;
        foreach (KeyValuePair<string, Dictionary<WeaponStats, float>> buffs in weaponbuffs_mul) {
            foreach (KeyValuePair<WeaponStats, float> data in buffs.Value) {
                if (data.Key == stat)
                    mul += data.Value;
            }
        }
        return plus * mul;
    }

    public float GetUnbuffedWeaponStat(float val, WeaponStats stat) {
        float plus = 0;
        foreach (KeyValuePair<string, Dictionary<WeaponStats, float>> buffs in weaponbuffs_plus) {
            foreach (KeyValuePair<WeaponStats, float> data in buffs.Value) {
                if (data.Key == stat)
                    plus += data.Value;
            }
        }
        float mul = 1;
        foreach (KeyValuePair<string, Dictionary<WeaponStats, float>> buffs in weaponbuffs_mul) {
            foreach (KeyValuePair<WeaponStats, float> data in buffs.Value) {
                if (data.Key == stat)
                    mul += data.Value;
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

    public void SetBaseStat(WeaponStats stat, float val) {
        weaponstats[(int)stat] = val;
    }

    public void ModStat(WeaponStats stat, float val) {
        weaponstats[(int)stat] = Mathf.Clamp(GetUnbuffedWeaponStat(GetBuffedWeaponStat(GetBaseStat(stat), stat) + val, stat), 0, GetMaxStat(stat));
    }

    public float GetBaseStat(WeaponStats stat) {
        return GameDataManager.instance.GetWeaponStat(GetWeapon().GetClass(), stat);
    }

    public float GetMaxStat(WeaponStats stat) {
        return GetBuffedWeaponStat(GetBaseStat(stat), stat);
    }

    public float GetCurrentStat(WeaponStats stat) {
        return GetBuffedWeaponStat(weaponstats[(int)stat], stat);
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
        if (weapon != null)
            Destroy(weapon.gameObject);
        GameObject gun_obj = (GameObject)Instantiate(Resources.Load("Prefab/Weapon"), transform.position, new Quaternion());
        weapon = (Weapon)gun_obj.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", classname, "ScriptClass")));
        Vector3 lscale = gun_obj.transform.localScale;
        lscale.x = gun_obj.transform.localScale.x * Mathf.Sign(transform.localScale.x);
        gun_obj.transform.localScale = lscale;
        gun_obj.transform.SetParent(transform);
        weapon.Initialize(classname);
        if (basecontroller != null)
            basecontroller.Initialize(this);
    }

    public Weapon GetWeapon() {
        return weapon;
    }

    public BasicCharacterMovement GetController() {
        return basecontroller;
    }

    public void DoDamage(Character attacker, float damage, float stagger) {
        if (damage == 0 || (flag & CharacterFlags.Invincible) == CharacterFlags.Invincible) return;
        if (IsAI())
            ((AIBaseController)basecontroller).OnTakeDamage(attacker);
        if (IsBoss()) {
            SetUncontrollable(true);
            StartCoroutine(Stagger(attacker.GetCurrentStat(WeaponStats.Stagger)));
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
