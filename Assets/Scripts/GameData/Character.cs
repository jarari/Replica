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
    Boss = 4,
    KnockBackImmunity = 8,
    StaggerImmunity = 16,
    UnstoppableAttack = 32
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
    protected Controller basecontroller;
    protected Weapon lastUsedWeapon = null;
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

    protected List<LootData> droptable = new List<LootData>();
    protected float lootThrowXMin = 50f;
    protected float lootThrowXMax = 150f;
    protected float lootThrowYMin = 300f;
    protected float lootThrowYMax = 450f;

    protected float colliderPushAmount = 50f;

    protected float maxJump = 0;
    protected float dashCooldown = 0.6f;
    protected float lastDash = 0;
    protected float dashDir = 1;
    protected float minDistToDash = -1f;
    protected float targetApproachRange;
    protected float subX = -1f;
    protected float grenadeCharge = 0f;
    protected float grenadeChargeRatio = 0f;
    protected float grenadeThrowMin = 0.25f;
    protected bool goingdown = false;
    protected bool satdown = false;

    protected float maxSpeed = 0f;
    protected float accel = 0f;
    protected float actualSpeed = 0f;
    protected float movedir = 0;
    protected float lastdir = 0;
    protected bool movedThisFrame = false;

    protected virtual void Update() {
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

        /* 위, 아래로 자유롭게 이동할 수 있도록 윗쪽 블럭과의 충돌을 미리 꺼놓음 */
        if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) > 0f) {
            foreach (Collider2D rayvel in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, (box.size.y / 2f + 16f) * Mathf.Sign(GetComponent<Rigidbody2D>().velocity.y)), new Vector2(2000, 8f), 0, Helper.groundLayer)) {
                if (rayvel != null) {
                    if (GetComponent<Rigidbody2D>().velocity.y > 0) {
                        Physics2D.IgnoreCollision(rayvel, box);
                        Physics2D.IgnoreCollision(rayvel, nofriction);
                    }
                    else if (!goingdown) {
                        Physics2D.IgnoreCollision(rayvel, box, false);
                        Physics2D.IgnoreCollision(rayvel, nofriction, false);
                    }
                }
            }
        }
        else {
            foreach (Collider2D rayup in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, (box.size.y / 2f + 16f)), new Vector2(2000, 8f), 0, Helper.groundLayer)) {
                if (rayup != null) {
                    Physics2D.IgnoreCollision(rayup, box);
                    Physics2D.IgnoreCollision(rayup, nofriction);
                }
            }
        }

        /* 조종 불가상태에서 돌아왔을 때 Idle 실행*/
        if (GetUncontrollableTimeLeft() == 0 && state == CharacterStates.Uncontrollable) {
            SetState(CharacterStates.Idle);
            anim.SetInteger("State", (int)CharacterStates.Idle);
        }

        /* 키보드 입력을 유지해야하는 동작들 (걷기, 달리기, 앉기)은 매 프레임마다 Idle로 리셋 */
        if (anim.GetInteger("State") == (int)CharacterStates.Walk
            || anim.GetInteger("State") == (int)CharacterStates.Sprint
            || anim.GetInteger("State") == (int)CharacterStates.Sit) {
            anim.SetInteger("State", (int)CharacterStates.Idle);
        }

        /* 투척 동작 관련
         * 투척 관련 자세가 아닌데 수류탄 게이지가 올라가있으면 캔슬당한 것으로 판정 */
        if (state != CharacterStates.Throw) {
            if (grenadeCharge != 0) {
                OnGrenadeCancelled();
            }
        }
        else {
            grenadeCharge = Mathf.Clamp(grenadeCharge + Time.deltaTime, 0, GetCurrentStat(CharacterStats.GrenadeFullCharge));
            grenadeChargeRatio = Mathf.Clamp(grenadeCharge / GetCurrentStat(CharacterStats.GrenadeFullCharge)
            , grenadeThrowMin, 1);
        }


        /* 앉았을 때 캐릭터 히트박스 감소 */
        if (state == CharacterStates.Sit) {
            ModifyHeight(0.65f);
            satdown = true;
        }
        else if(satdown){
            ModifyHeight(1f);
            satdown = false;
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        Collider2D[] colliders = new Collider2D[64];
        Physics2D.OverlapBoxNonAlloc((Vector2)transform.position + box.offset, box.size, 0, colliders, Helper.characterLayer);
        foreach (Collider2D col in colliders) {
            if(col != null) {
                if (col.gameObject != gameObject) {
                    Character c = col.GetComponent<Character>();
                    if (c && !c.GetClass().Equals(GetClass())) {
                        float xDiff = (transform.position.x - c.transform.position.x);
                        int pushDir = 1;
                        if (xDiff > 0) {
                            pushDir = -1;
                        }
                        if (Mathf.Abs(c.GetRigidbody().velocity.x) < colliderPushAmount) {
                            c.AddForce(Vector2.right * pushDir * colliderPushAmount);
                        }
                    }
                }
            }
        }
    }

    public override void Initialize(string classname) {
        base.Initialize(classname);

        for (int i = 0; i < (int)CharacterStats.EndOfEnums; i++)
            stats[i] = GetBaseStat((CharacterStats)i);

        GetComponent<CircleCollider2D>().offset = new Vector2(0, box.offset.y - box.size.y / 2f + 0.05f);
        GetComponent<CircleCollider2D>().radius = 0.6f;

		if(objectData["Sprites"]["hasSub"])
			hasSubSprite = objectData["Sprites"]["hasSub"].Value<int>();

		if (objectData["KnockBackImmunity"])
            if (objectData["KnockBackImmunity"].Value<float>() == 1)
                SetFlag(CharacterFlags.KnockBackImmunity);

        if (objectData["DropTable"]) {
            foreach(JDictionary subDict in objectData["DropTable"]) {
                float chance = subDict["Chance"].Value<float>();
                int nmin = subDict["NumMin"].Value<int>();
                int nmax = subDict["NumMax"].Value<int>();
				droptable.Add(new LootData(subDict.Key, chance, nmin, nmax));
            }
        }

        lastDash = Time.time - dashCooldown;
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

    public Teams GetTeam() {
        return team;
    }

    public void SetTeam(Teams t) {
        team = t;
    }

    public string GetName() {
        return objectData["Name"].Value<string>();
    }

    public int HasSubSprite() {
        return hasSubSprite;
    }

    public void SetBaseStat(CharacterStats stat, float val) {
        stats[(int)stat] = val;
    }

    public void ModStat(CharacterStats stat, float val) {
        stats[(int)stat] = Mathf.Clamp(GetUnbuffedStat(GetBuffedStat(stats[(int)stat], stat) + val, stat), 0, GetMaxStat(stat));
        if (stat == CharacterStats.Health) {
            OnHealthChanged(val);
        }
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
        weaponstats[wep.GetWeaponType()][(int)stat] = Mathf.Clamp(GetUnbuffedWeaponStat(GetBuffedWeaponStat(weaponstats[wep.GetWeaponType()][(int)stat], wep.GetWeaponType(), stat) + val, wep.GetWeaponType(), stat), 0, GetMaxStat(wep, stat));
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

    public virtual void KnockDown() {

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

    public void SetController(Controller controller) {
        basecontroller = controller;
    }

    public CharacterFlags GetFlag() {
        return flag;
    }

    public bool HasFlag(CharacterFlags _flag) {
        return (flag & _flag) != 0;
    }

    public bool IsPlayer() {
        return !HasFlag(CharacterFlags.AIControlled);
    }

    public bool IsAI() {
        return HasFlag(CharacterFlags.AIControlled);
    }

    public bool IsBoss() {
        return HasFlag(CharacterFlags.Boss);
    }

    public bool IsInvincible() {
        return HasFlag(CharacterFlags.Invincible);
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

		string script = GameDataManager.instance.RootData[classname]["ScriptClass"].Value<string>();
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

		EventManager.OnWeaponEquipped(this, wep);
    }

    public void RemoveWeapon(WeaponTypes type) {
        if (weapons.ContainsKey(type)) {
			EventManager.OnWeaponUnEquipped(this, weapons[type]);
            Destroy(weapons[type].gameObject);
            weapons.Remove(type);
        }
    }

    public Weapon GetWeapon(WeaponTypes type) {
        return weapons[type];
    }

    public Weapon GetLastUsedWeapon() {
        return lastUsedWeapon;
    }

    public Controller GetController() {
        return basecontroller;
    }

    public void DoDamage(Character attacker, float damage, float stagger) {
        if (damage == 0 || HasFlag(CharacterFlags.Invincible)) return;

        if (IsAI() && !IsBoss())
            ((AIBaseController)basecontroller).OnTakeDamage(attacker);

		if (stagger > 0) {
            OnStagger(stagger);

			EventManager.OnCharacterStagger(this, attacker, stagger);
        }

		ModStat(CharacterStats.Health, -damage);

		EventManager.OnCharacterHit(this, attacker, damage, stagger);

		if (GetCurrentStat(CharacterStats.Health) == 0) {
            OnDeath();

			EventManager.OnCharacterKilled(this, attacker);
        }
    }

    public void Kill() {
        DoDamage(this, GetCurrentStat(CharacterStats.Health), 0);
    }

    public void DestroyQuietly() {
        CharacterManager.OnCharacterDeath(this);
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
        if (GetInventory() != null) {
            Destroy(GetInventory());
            InventoryManager.DestroyInventory(gameObject);
        }
        if (basecontroller != null) {
            Destroy(basecontroller);
        }
    }

    protected virtual void OnStagger(float stagger) {
        if (HasFlag(CharacterFlags.StaggerImmunity))
            return;
        AddUncontrollableTime(stagger);
    }

    protected virtual void OnHealthChanged(float delta) {
		EventManager.OnCharacterHealthChanged(this, delta);
    }

    protected virtual void OnDeath() {
        if (transform == null)
            return;
        if (IsAI()) {
            if(droptable.Count > 0) {
                foreach (LootData loot in droptable) {
                    if (Helper.CalcChance(loot.chance)) {
                        int count = UnityEngine.Random.Range(loot.numMin, loot.numMax + 1);
                        int direction = (int)((UnityEngine.Random.Range(0, 2) - 0.5f) * 2f);
                        float throwX = UnityEngine.Random.Range(lootThrowXMax, lootThrowXMax);
                        float throwY = UnityEngine.Random.Range(lootThrowYMin, lootThrowYMax);
                        LootManager.CreateLoot(loot.item, count, transform.position, 0, new Vector2(throwX * direction, throwY));
                    }
                }
            }
        }
        DestroyQuietly();
    }

    /* 이동속도에 비례해서 움직이기.
     * a 값으로 속도를 조절할 수는 있지만 최대 속도를 넘어설 수는 없음 */
    public virtual void Move(float a) {
        movedThisFrame = true;
        lastdir = movedir;
        movedir = a;
        accel = GetCurrentStat(CharacterStats.MoveSpeed) / 10f;
        maxSpeed = GetCurrentStat(CharacterStats.MoveSpeed);
        actualSpeed = Mathf.Clamp(actualSpeed + accel, 0, maxSpeed);
        if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) < Mathf.Abs(movedir * actualSpeed)) {
            float velX = Mathf.Clamp(movedir * actualSpeed, -maxSpeed, maxSpeed);
            GetComponent<Rigidbody2D>().velocity = new Vector2(velX, GetComponent<Rigidbody2D>().velocity.y);
        }
        else if (movedir * GetComponent<Rigidbody2D>().velocity.x < 0) {
            float velX = GetComponent<Rigidbody2D>().velocity.x + movedir * actualSpeed;
            GetComponent<Rigidbody2D>().velocity = new Vector2(velX, GetComponent<Rigidbody2D>().velocity.y);
        }
    }

    /* 최고 속도로 즉시 움직일 떄 사용 */
    public void ForceMove(float a) {
        maxSpeed = GetCurrentStat(CharacterStats.MoveSpeed);
        actualSpeed = maxSpeed;
        Move(a);
    }

    /* 밀어내기 */
    public void AddForce(Vector2 force) {
        GetComponent<Rigidbody2D>().velocity += force;
    }

    public void AddForce(Vector2 force, bool knockdown = false) {
        AddForce(force);
        if (knockdown) {
            if (force.x > 0)
                FlipFace(false);
            else if (force.x < 0)
                FlipFace(true);
            KnockDown();
        }
    }

    /* y 가속도 강제 오버라이드 */
    public void SetVelY(float v) {
        GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, v);
    }

    protected override void LateUpdate() {
        base.LateUpdate();
        if (GetUncontrollableTimeLeft() > 0 || !movedThisFrame || lastdir * movedir < 0) {
            actualSpeed = 0.0f;
            return;
        }
        movedThisFrame = false;
        movedir = 0;
    }

    public virtual void Walk(float dir) {
        if (GetUncontrollableTimeLeft() > 0
            || GetState() == CharacterStates.Attack)
            return;
        GetAnimator().SetInteger("State", (int)CharacterStates.Walk);
        if (GetState() != CharacterStates.Sit
            && GetState() != CharacterStates.Throw)
            Move(dir * 0.6f);
        if (dir > 0) {
            FlipFace(true);
        }
        else if (dir < 0) {
            FlipFace(false);
        }
        if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("walk")) {
            SetState(CharacterStates.Walk);
        }
    }

    public virtual void Sprint(float dir) {
        if (GetUncontrollableTimeLeft() > 0
            || GetState() == CharacterStates.Attack)
            return;
        GetAnimator().SetInteger("State", (int)CharacterStates.Sprint);
        if (GetState() != CharacterStates.Sit
            && GetState() != CharacterStates.Throw
            && IsOnGround()) {
            SetState(CharacterStates.Sprint);
            Move(dir);
            if (dir > 0) {
                FlipFace(true);
            }
            else if (dir < 0) {
                FlipFace(false);
            }
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("jump") || GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fall")) {
            Move(dir * 0.6f);
            if (dir == 1) {
                FlipFace(true);
            }
            else if (dir == -1) {
                FlipFace(false);
            }
        }
    }


    protected bool CanJump() {
        if (GetUncontrollableTimeLeft() > 0)
            return false;
        if (!IsOnGround())
            return false;
        return true;
    }

    public virtual void Jump() {
        if (!CanJump())
            return;
        GetAnimator().SetInteger("State", (int)CharacterStates.Jump);
    }

    protected void OnJumpEvent() {
        SetState(CharacterStates.Jump);
        AddForce(Vector3.up * GetCurrentStat(CharacterStats.JumpPower));
        GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnFallEvent() {
        if (GetState() != CharacterStates.Jump) {
            SetState(CharacterStates.Jump);
            GetAnimator().SetInteger("State", (int)CharacterStates.Jump);
            GetAnimator().SetBool("DiscardFromAnyState", true);
        }
    }

    protected void OnLandEvent() {
        SetState(CharacterStates.Idle);
        GetAnimator().SetBool("DiscardFromAnyState", false);
        GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
    }

    /* 이름이 대쉬인데 덤블링임 사실 */
    protected bool CanDash() {
        if (GetUncontrollableTimeLeft() > 0)
            return false;
        if (!IsOnGround())
            return false;
        if (Time.time - lastDash < dashCooldown)
            return false;
        return true;
    }

    /* 방향이 입력이 없다면 백덤블링
     * 있다면 해당 방향으로 덤블링 */
    public virtual void Dash(int dir) {
        if (Time.time - lastDash < dashCooldown)
            return;

        if (facingRight) {
            if (dir == 1) {
                anim.Play("tumble_front");
                dashDir = 1;
            }
            else {
                anim.Play("tumble_back");
                dashDir = -1;
            }
        }
        else {
            if (dir == 1) {
                anim.Play("tumble_back");
                dashDir = 1;
            }
            else {
                anim.Play("tumble_front");
                dashDir = -1;
            }
        }
        OnTumbleEvent();
    }

    /* 덤블링 높이는 점프력에 비례함 */
    protected void OnTumbleEvent() {
        if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("tumble_back")) {
            EffectManager.CreateEffect("effect_tumble_back", transform.position, 0, null, !IsFacingRight());
            EffectManager.CreateEffect("effect_tumble_back2", transform.position, 0, null, !IsFacingRight());
            EffectManager.CreateEffect("effect_tumble_backf", transform.position, 0, transform, !IsFacingRight());
            ParticleManager.instance.CreateParticle("particle_tumb", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
            ParticleManager.instance.CreateParticle("particle_tumb2", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
            ParticleManager.instance.CreateParticle("particle_tumbb", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
            ParticleManager.instance.CreateParticle("particle_tumbb", transform.position, (int)Mathf.Sign(transform.localScale.x), !IsFacingRight());
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("tumble_front")) {
            EffectManager.CreateEffect("effect_tumble_front", transform.position, 0, null, !IsFacingRight());
            EffectManager.CreateEffect("effect_tumble_front2", transform.position, 0, null, !IsFacingRight());
            EffectManager.CreateEffect("effect_tumble_frontf", transform.position, 0, transform, !IsFacingRight());
            ParticleManager.instance.CreateParticle("particle_tumb", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
            ParticleManager.instance.CreateParticle("particle_tumb2", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
            ParticleManager.instance.CreateParticle("particle_tumbf", transform.position, (int)Mathf.Sign(transform.localScale.x), transform);
            ParticleManager.instance.CreateParticle("particle_tumbf", transform.position, (int)Mathf.Sign(transform.localScale.x), !IsFacingRight());
        }

        lastDash = Time.time;
        AddUncontrollableTime(0.15f);
        ForceMove(dashDir);
        AddForce(Vector3.up * GetCurrentStat(CharacterStats.JumpPower) * 0.15f
            + Vector3.right * dashDir * GetCurrentStat(CharacterStats.JumpPower) * 0.1f);
        SetFlag(CharacterFlags.Invincible);
        gameObject.layer = LayerMask.NameToLayer("CharactersShifting");
        GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnTumbleInvincibilityEndEvent() {
        RemoveFlag(CharacterFlags.Invincible);
        gameObject.layer = LayerMask.NameToLayer("Characters");
    }

    protected void OnTumbleEndEvent() {
        SetState(CharacterStates.Idle);
        GetAnimator().SetBool("DiscardFromAnyState", false);
        GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
    }

    public void Sit() {
        if (GetState() == CharacterStates.Uncontrollable)
            return;
        GetAnimator().SetInteger("State", (int)CharacterStates.Sit);
    }

    /* 앉으면 가속도 초기화 (슬라이딩 방지)*/
    protected void OnSit() {
        ForceMove(0);
        SetState(CharacterStates.Sit);
        GetAnimator().SetBool("DiscardFromAnyState", true);
    }

    protected void OnSitLoop() {
        SetState(CharacterStates.Sit);
    }

    protected void OnStandup() {
        SetState(CharacterStates.Idle);
        GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    /* 맵 레이어는 Ground와 Ceiling으로 나뉘는데,
     * Ceiling은 어떤 상황에서도 뚫리지 않는 지형
     * Ground는 자유자재로 위아래로 돌아다닐 수 있는 지형 */
    protected bool CanGoDown() {
        return Physics2D.OverlapBox((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f + 16f), new Vector2(box.size.x, 16f), 0, Helper.ceilingLayer) == null;
    }

    public void GoDown() {
        if (!CanGoDown())
            return;
        goingdown = true;
        StartCoroutine(GoDownEnd());
        foreach (Collider2D raydown in Physics2D.OverlapBoxAll((Vector2)transform.position + box.offset + new Vector2(0, -box.size.y / 2f), new Vector2(2000, 64f), 0, Helper.groundLayer)) {
            if (raydown != null) {
                Physics2D.IgnoreCollision(raydown, box);
                Physics2D.IgnoreCollision(raydown, nofriction);
            }
        }
    }
    protected IEnumerator GoDownEnd() {
        yield return new WaitWhile(() => IsOnGround());
        yield return new WaitWhile(() => !IsOnGround() || GetComponentInParent<Rigidbody2D>().velocity.y < -1f);
        goingdown = false;
    }

    protected void OnIdle() {
        SetState(CharacterStates.Idle);
        GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    /* 공격 이벤트 */
    protected virtual void OnAttackEvent(string eventname) {
        GetAnimator().SetInteger("State", (int)CharacterStates.Attack);
        SetState(CharacterStates.Attack);
        if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("gunkata")) {
            GetWeapon(WeaponTypes.Pistol).OnAttack(eventname);
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fist")) {
            GetWeapon(WeaponTypes.Fist).OnAttack(eventname);
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("sword")) {
            GetWeapon(WeaponTypes.Sword).OnAttack(eventname);
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("ai")) {
            GetWeapon(WeaponTypes.AI).OnAttack(eventname);
        }
    }

    /* 기타 무기 이벤트 (타이밍에 맞게 특정 행동을 하기 위함) */
    protected virtual void OnWeaponEvent(string eventname) {
        if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("gunkata")) {
            GetWeapon(WeaponTypes.Pistol).OnWeaponEvent(eventname);
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("fist")) {
            GetWeapon(WeaponTypes.Fist).OnWeaponEvent(eventname);
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("sword")) {
            GetWeapon(WeaponTypes.Sword).OnWeaponEvent(eventname);
        }
        else if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("ai")) {
            GetWeapon(WeaponTypes.AI).OnWeaponEvent(eventname);
        }
    }

    /* 캐릭터에 부착된 이펙트 생성 (애니메이션 이벤트용) */
    protected void OnParentedEffectEvent(string effect) {
        EffectManager.CreateEffect(effect, transform.position, 0, transform, false);
    }

    /* 캐릭터와 독립된 이펙트 생성 (애니메이션 이벤트용) */
    protected void OnEffectEvent(string effect) {
        EffectManager.CreateEffect(effect, transform.position, 0, null, !IsFacingRight());
    }

    protected virtual void OnChargeGrenade() {
        GetAnimator().SetInteger("State", (int)CharacterStates.Throw);
        SetState(CharacterStates.Throw);
    }

    protected virtual void OnGrenadeCancelled() {
        if (GetState() == CharacterStates.Throw)
            GetAnimator().Play("idle_loop");
        grenadeCharge = 0;
        grenadeChargeRatio = 0;
    }

    /* 수류탄 투척 함수.
     * 현재는 무조건 기본 수류탄이 나가도록 돼있지만
     * 차후 인벤토리에서 수류탄 갯수를 확인하여 해당 수류탄을 던질 수 있도록 바꿀 계획 */
    protected virtual void OnThrowGrenade(string className) {
        JDictionary grenadeData = GameDataManager.instance.RootData[className];

        //Get grenade class, not yet implemented for now.
        Vector2 throwpos = (Vector2)transform.position +
            new Vector2(
                grenadeData["MuzzlePos"]["X"].Value<float>() * GetFacingDirection(),
                grenadeData["MuzzlePos"]["Y"].Value<float>()
                );
        float throwang = grenadeData["ThrowAngle"].Value<float>();
        GiveWeapon("weapon_grenade");
        Weapon grenade = GetWeapon(WeaponTypes.Throwable);
        BulletManager.CreateThrowable("throwable_grenade", throwpos, this, grenade,
            GetCurrentStat(CharacterStats.GrenadeThrowPower) * grenadeChargeRatio,
            GetCurrentStat(grenade, WeaponStats.ExplosionRadius), 90 - (90 - throwang) * GetFacingDirection(), 300,
            grenade.GetEssentialStats());
        RemoveWeapon(WeaponTypes.Throwable);
        SetState(CharacterStates.Idle);
    }

    /* 피격 이벤트 */
    protected virtual void OnHitEvent(int invincible) {
        GetAnimator().SetInteger("State", 8);
        SetState(CharacterStates.Uncontrollable);
        if (invincible == 1)
            SetFlag(CharacterFlags.Invincible);
        GetAnimator().SetBool("DiscardFromAnyState", true);
        if (GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("knockout"))
            StartCoroutine(OnKnockoutRecoverEvent(invincible));
        else
            StartCoroutine(OnHitRecoverEvent(GetUncontrollableTimeLeft() + 0.25f, invincible));
        if (IsAI() && !IsBoss())
            basecontroller.ResetAttackTimer();
    }

    /* 피격 회복 이벤트 */
    protected IEnumerator OnHitRecoverEvent(float delay, int invincible) {
        yield return new WaitForSeconds(delay);
        if (invincible == 1)
            RemoveFlag(CharacterFlags.Invincible);
        if (GetAnimator().GetInteger("State") == 8)
            GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    protected IEnumerator OnKnockoutRecoverEvent(int invincible) {
        yield return new WaitWhile(() => GetAnimator().GetCurrentAnimatorStateInfo(0).IsTag("knockout"));
        SetUncontrollable(false);
        if (invincible == 1)
            RemoveFlag(CharacterFlags.Invincible);
        GetAnimator().SetBool("DiscardFromAnyState", false);
    }

    /* 다양한 용도로 쓸 수 있는 좌표를 향해 가기 함수
     * Navmesh같은걸 이용한다면 좀 더 멋드러지고 효과적이겠지만
     * 일단은 이 알고리즘을 쓰도록 함. */
    public virtual void Follow(Vector3 pos, float xradius) {
        float dx = pos.x - transform.position.x;
        float dy = pos.y - transform.position.y;
        float dist = Mathf.Abs(dx);
        int dir = (int)Mathf.Sign(dx);
        maxJump = Mathf.Pow(GetCurrentStat(CharacterStats.JumpPower), 2) / 3924f;
        if (GetUncontrollableTimeLeft() == 0) {
            GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
            RaycastHit2D rayup = Physics2D.Raycast(transform.position, new Vector2(0, 1), maxJump, Helper.groundLayer);
            RaycastHit2D rayunder = Physics2D.Raycast((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f + 33), new Vector2(0, -1), 128f, Helper.groundLayer);
            if (dist > xradius) {
                subX = -1;
                if (minDistToDash != -1 && dist > minDistToDash && CanDash()) {
                    Dash(dir);
                }
                else {
                    Walk(dir);
                }
            }
            else {
                if (dy > 1f) {
                    if (rayup.collider == null) {
                        if (subX == -1) {
                            float minXDist = 999999;
                            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Ground")) {
                                float odx = obj.transform.position.x - transform.position.x;
                                float ody = obj.transform.position.y - transform.position.y;
                                if (ody < 0 || ody > maxJump || odx * dx < 0 || Mathf.Abs(odx) >= 600)
                                    continue;
                                else {
                                    if (minXDist > Mathf.Abs(odx)) {
                                        subX = obj.transform.position.x;
                                        minXDist = Mathf.Abs(odx);
                                    }
                                }
                            }
                        }
                    }
                    else if (subX != -1) {
                        if (Mathf.Abs(subX - transform.position.x) < 3f) {
                            subX = -1;
                            return;
                        }
                        dir = (int)Mathf.Sign(subX - transform.position.x);
                        Walk(dir);
                    }
                    else {
                        Walk(dir);
                    }
                }
                else if (dy < -1f) {
                    if (rayunder.collider == null && subX == -1) {
                        float minXDist = 999999;
                        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Ground")) {
                            float odx = obj.transform.position.x - transform.position.x;
                            float ody = obj.transform.position.y - transform.position.y;
                            if (ody > -32 || odx * dx < 0 || Mathf.Abs(odx) >= 600)
                                continue;
                            else {
                                if (minXDist > Mathf.Abs(odx)) {
                                    subX = obj.transform.position.x;
                                    minXDist = Mathf.Abs(odx);
                                }
                            }
                        }
                    }
                    else if (subX != -1) {
                        if (Mathf.Abs(subX - transform.position.x) < 3f) {
                            subX = -1;
                            return;
                        }
                        dir = (int)Mathf.Sign(subX - transform.position.x);
                        Walk(dir);
                    }
                    else {
                        Walk(dir);
                    }
                }
            }
            if (Physics2D.OverlapBox((Vector2)transform.position + box.offset + new Vector2(box.size.x / 2f * GetFacingDirection(), 4 - box.size.y / 2f), new Vector2(16, 5), 0, Helper.mapLayer) != null
                            && Physics2D.OverlapBox((Vector2)transform.position + box.offset + new Vector2(box.size.x / 2f * GetFacingDirection(), maxJump - box.size.y / 2f), new Vector2(16, 5), 0, Helper.mapLayer) == null) {
                Jump();
            }
            if (Mathf.Abs(dy) > 32f && IsOnGround() && GetState() != CharacterStates.Jump) {
                if (dy < -32) {
                    if (CanGoDown()) {
                        GoDown();
                    }
                }
                else if (rayup.collider != null && dy > 32) {
                    Jump();
                    subX = -1;
                }
            }
        }
        else {
            GetAnimator().SetInteger("State", (int)CharacterStates.Idle);
        }
    }
}
