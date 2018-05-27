using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponStats {
    Damage,
    SADestruction,
    Spread,
    AttackSpeed,
    Stagger,
    MagSize,
    ReloadTime,
    Range,
    BulletSpeed,
    EndOfEnums
}

public enum WeaponTypes {
    Fist,
    Pistol,
    Sword,
    Throwable,
    AI
}

public abstract class Weapon : ObjectBase {

    protected WeaponTypes type;
    protected Character owner;
    protected LayerMask characterLayer;
    public Vector2 muzzlePos;

    public override void Initialize(string classname) {
        base.Initialize(classname);
        type = (WeaponTypes)Convert.ToInt32(GameDataManager.instance.GetData("Data", classname, "Stats", "WeaponType"));
        if (owner) {
            for (int i = 0; i < (int)WeaponStats.EndOfEnums; i++) {
                owner.SetBaseStat(this, (WeaponStats)i, owner.GetBaseStat(this, (WeaponStats)i));
            }
            GetComponent<SpriteRenderer>().sortingOrder = GetOwner().GetComponent<SpriteRenderer>().sortingOrder + 1;
        }
        characterLayer = (1 << LayerMask.NameToLayer("Characters"));
        muzzlePos = new Vector2((float)GameDataManager.instance.GetData("Data", classname, "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData("Data", classname, "MuzzlePos", "Y"));
    }
    public Vector2 GetMuzzlePos() {
        return (Vector2)transform.position + new Vector2(muzzlePos.x * owner.GetFacingDirection(), muzzlePos.y);
    }

    public void SetMuzzlePos(Vector2 newv) {
        muzzlePos = newv;
    }

    public Character GetOwner() {
        return owner;
    }

    public void SetOwner(Character c) {
        owner = c;
    }

    public WeaponTypes GetWeaponType() {
        return type;
    }

    public Dictionary<WeaponStats, float> GetEssentialStats() {
        Dictionary<WeaponStats, float> stats = new Dictionary<WeaponStats, float>();
        stats.Add(WeaponStats.AttackSpeed, owner.GetCurrentStat(this, WeaponStats.AttackSpeed));
        stats.Add(WeaponStats.Damage, owner.GetCurrentStat(this, WeaponStats.Damage));
        stats.Add(WeaponStats.SADestruction, owner.GetCurrentStat(this, WeaponStats.SADestruction));
        stats.Add(WeaponStats.Stagger, owner.GetCurrentStat(this, WeaponStats.Stagger));
        stats.Add(WeaponStats.BulletSpeed, owner.GetCurrentStat(this, WeaponStats.BulletSpeed));
        stats.Add(WeaponStats.Range, owner.GetCurrentStat(this, WeaponStats.Range));
        return stats;
    }

    public virtual void OnAttack(string eventname) {
        owner.GetAnimator().SetInteger("State", (int)CharacterStates.Attack);
        owner.SetState(CharacterStates.Attack);
        Vector2 knockback = new Vector2();
        if(GameDataManager.instance.GetData("Data", eventname, "ChargeAmount") != null) {
            owner.GetController().ForceMove(0);
            knockback.x = Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "ChargeAmount")) * owner.GetFacingDirection();
            owner.AddForce(Vector2.right * knockback.x);
        }
        if (GameDataManager.instance.GetData("Data", eventname, "KnockBack") != null) {
            knockback.x = Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "KnockBack", "X")) * owner.GetFacingDirection();
            knockback.y = Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "KnockBack", "Y"));
        }
        Dictionary<WeaponStats, float> dmgMult = new Dictionary<WeaponStats, float>();
        dmgMult.Add(WeaponStats.Damage, Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "DamageMultiplier")));
        owner.AddWeaponBuff("buff_damage_" + eventname, dmgMult, false, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);

        Dictionary<WeaponStats, float> ccAdd = new Dictionary<WeaponStats, float>();
        ccAdd.Add(WeaponStats.SADestruction, Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "SADestruction")));
        ccAdd.Add(WeaponStats.Stagger, Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "Stagger")));
        owner.AddWeaponBuff("buff_cc_" + eventname, ccAdd, true, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);
        
        if(GameDataManager.instance.GetData("Data", eventname, "HitBox") != null
            && GameDataManager.instance.GetData("Data", eventname, "HitBox").GetType().Equals(typeof(Dictionary<string, object>))){
            int hitboxnum = ((Dictionary<string, object>)GameDataManager.instance.GetData("Data", eventname, "HitBox")).Count;
            for(int i = 0; i < hitboxnum; i++) {
                AnimationClip clip = owner.GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip;
                float time = Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "HitBox", i.ToString(), "Time"));
                float numOfFrames = Mathf.Round(clip.frameRate * clip.length);
                Vector2 pos = new Vector2(Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "HitBox", i.ToString(), "Pos", "X")) * owner.GetFacingDirection(),
                    Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "HitBox", i.ToString(), "Pos", "Y")));
                Vector2 area = new Vector2(Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "HitBox", i.ToString(), "Area", "X")),
                    Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "HitBox", i.ToString(), "Area", "Y")));
                StartCoroutine(HitCheck(time / numOfFrames, pos, area, knockback));
            }
        }
        /*Dictionary<CharacterStats, float> moveSpeedDebuff = new Dictionary<CharacterStats, float>();
        moveSpeedDebuff.Add(CharacterStats.MoveSpeed, -99);
        owner.AddBuff("debuff_ms_attack", moveSpeedDebuff, false, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);*/
    }

    IEnumerator HitCheck(float normalizedtime, Vector2 localPos, Vector2 area, Vector2 knockback) {
        yield return new WaitWhile(() => owner.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime < normalizedtime);
        List<Character> closeEnemies = CharacterManager.instance.GetEnemies(GetOwner().GetTeam()).FindAll
            (c => Helper.IsInBox(c.transform.position, (Vector2)owner.transform.position + localPos - area / 2f, (Vector2)owner.transform.position + localPos + area / 2f));
        StartCoroutine(DrawBox((Vector2)owner.transform.position + localPos, area));
        foreach (Character c in closeEnemies) {
            c.AddForce(knockback);
            float hitposX = Mathf.Clamp(((Vector2)owner.transform.position + localPos).x,
                c.transform.position.x + c.GetCollider().offset.x - c.GetCollider().size.x / 2f,
                c.transform.position.x + c.GetCollider().offset.x + c.GetCollider().size.x / 2f);
            float hitposY = Mathf.Clamp(((Vector2)owner.transform.position + localPos).y,
                c.transform.position.y + c.GetCollider().offset.y - c.GetCollider().size.y / 2f,
                c.transform.position.y + c.GetCollider().offset.y + c.GetCollider().size.y / 2f);
            OnWeaponHit(new Vector2(hitposX, hitposY));
        }
    }

    IEnumerator DrawBox(Vector2 pos, Vector2 area) {
        float elapsed = 0;
        float duration = 0.5f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            float minX = pos.x - area.x / 2f;
            float maxX = pos.x + area.x / 2f;
            float minY = pos.y - area.y / 2f;
            float maxY = pos.y + area.y / 2f;
            Debug.DrawLine(new Vector2(minX, minY), new Vector2(minX, maxY));
            Debug.DrawLine(new Vector2(minX, maxY), new Vector2(maxX, maxY));
            Debug.DrawLine(new Vector2(maxX, minY), new Vector2(maxX, maxY));
            Debug.DrawLine(new Vector2(minX, minY), new Vector2(maxX, minY));

            yield return null;
        }
    }

    public virtual void OnWeaponHit(Vector2 hitPos) {
        EffectManager.instance.CreateEffect("effect_indicator_armorpen", hitPos, 0);
    }

    public abstract void OnWeaponEvent(string eventname);
}