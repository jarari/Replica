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
    ExplosionRadius,
    EndOfEnums
}

public enum WeaponTypes {
    Fist,
    Pistol,
    Sword,
    Throwable,
    AI
}

public class Weapon : ObjectBase {

    protected WeaponTypes type;
    protected Character owner;
    protected string bullet;
    public Vector2 muzzlePos;

    public override void Initialize(string classname) {
        base.Initialize(classname);
        type = (WeaponTypes)Convert.ToInt32(GameDataManager.instance.GetData(classname, "Stats", "WeaponType"));
        if (owner) {
            for (int i = 0; i < (int)WeaponStats.EndOfEnums; i++) {
                owner.SetBaseStat(this, (WeaponStats)i, owner.GetBaseStat(this, (WeaponStats)i));
            }
            GetComponent<SpriteRenderer>().sortingOrder = GetOwner().GetComponent<SpriteRenderer>().sortingOrder + 1;
        }
        muzzlePos = new Vector2((float)GameDataManager.instance.GetData(classname, "MuzzlePos", "X")
                                        , (float)GameDataManager.instance.GetData(classname, "MuzzlePos", "Y"));
        SetBullet();
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

    public void SetBullet(string b = null) {
        if(b == null || b.Length == 0) {
            if(GameDataManager.instance.GetData(className, "Stats", "DefaultBullet") != null) {
                bullet = (string)GameDataManager.instance.GetData(className, "Stats", "DefaultBullet");
            }
            else {
                bullet = "";
            }
        }
        else {
            bullet = b;
        }
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
        stats.Add(WeaponStats.ExplosionRadius, owner.GetCurrentStat(this, WeaponStats.ExplosionRadius));
        return stats;
    }

    public void FireBullet(string bulletclass, float ang) {
        if ((string)GameDataManager.instance.GetData(bulletclass, "Type") == "bullet")
            BulletManager.instance.CreateBullet(bulletclass, GetMuzzlePos(), owner, this, 90 - ang * owner.GetFacingDirection(), GetEssentialStats());
        else if ((string)GameDataManager.instance.GetData(bulletclass, "Type") == "laser")
            BulletManager.instance.CreateLaser(bulletclass, GetMuzzlePos(), owner, this, 90 - ang * owner.GetFacingDirection(), GetEssentialStats());
        if (EventManager.Event_WeaponFire != null)
            EventManager.Event_WeaponFire(owner, this, bulletclass);
    }

    public void CreateEffect(string effectname) {
        EffectManager.instance.CreateEffect(effectname, transform.position, owner.GetFacingDirection());
    }

    public virtual void OnAttack(string eventname) {
        if (!owner.IsOnGround()) {
            if(owner.GetComponent<Rigidbody2D>().velocity.y < 300)
                owner.GetController().SetVelY(300);
        }
        if (GameDataManager.instance.GetData(eventname, "Unstoppable") != null &&
            Convert.ToInt32(GameDataManager.instance.GetData(eventname, "Unstoppable")) == 1) {
            owner.SetFlag(CharacterFlags.UnstoppableAttack);
        }
        if (GameDataManager.instance.GetData(eventname, "ChargeDelay") != null) {
            AnimationClip clip = owner.GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip;
            float time = Convert.ToSingle(GameDataManager.instance.GetData(eventname, "ChargeDelay"));
            float numOfFrames = Mathf.Round(clip.frameRate * clip.length);
            StartCoroutine(DelayedCharge(time / numOfFrames, Convert.ToSingle(GameDataManager.instance.GetData(eventname, "ChargeAmount"))));
        }
        else {
            if (GameDataManager.instance.GetData(eventname, "ChargeAmount") != null) {
                owner.GetController().ForceMove(0);
                owner.AddForce(Vector2.right * Convert.ToSingle(GameDataManager.instance.GetData(eventname, "ChargeAmount")) * owner.GetFacingDirection());
            }
        }
        Dictionary<WeaponStats, float> dmgMult = new Dictionary<WeaponStats, float>();
        float mult = Convert.ToSingle(GameDataManager.instance.GetData(eventname, "DamageMultiplier")) - 1;
        if(mult != 0) {
            dmgMult.Add(WeaponStats.Damage, mult);
            WeaponBuff dmgMultBuff = new WeaponBuff(dmgMult, type);
            owner.AddWeaponBuff("buff_damage_" + eventname, dmgMultBuff, false, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);
        }

        Dictionary<WeaponStats, float> ccAdd = new Dictionary<WeaponStats, float>();
        ccAdd.Add(WeaponStats.SADestruction, Convert.ToSingle(GameDataManager.instance.GetData(eventname, "SADestruction")));
        ccAdd.Add(WeaponStats.Stagger, Convert.ToSingle(GameDataManager.instance.GetData(eventname, "Stagger")));
        if (GameDataManager.instance.GetData(eventname, "ExplosionRadius") != null)
            ccAdd.Add(WeaponStats.ExplosionRadius, Convert.ToSingle(GameDataManager.instance.GetData(eventname, "ExplosionRadius")));
        WeaponBuff ccAddBuff = new WeaponBuff(ccAdd, type);
        owner.AddWeaponBuff("buff_cc_" + eventname, ccAddBuff, true, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);
        
        if(GameDataManager.instance.GetData(eventname, "HitBox") != null
            && GameDataManager.instance.GetData(eventname, "HitBox").GetType().Equals(typeof(Dictionary<string, object>))){
            int hitboxnum = ((Dictionary<string, object>)GameDataManager.instance.GetData(eventname, "HitBox")).Count;
            for(int i = 0; i < hitboxnum; i++) {
                AnimationClip clip = owner.GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip;
                float time = Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Time"));
                float numOfFrames = Mathf.Round(clip.frameRate * clip.length);
                Vector2 pos = new Vector2(Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Pos", "X")) * owner.GetFacingDirection(),
                    Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Pos", "Y")));
                Vector2 area = new Vector2(Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Area", "X")),
                    Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Area", "Y")));
                StartCoroutine(HitCheck(time / numOfFrames, pos, area, eventname));
            }
        }
        if (EventManager.Event_WeaponAttack != null)
            EventManager.Event_WeaponAttack(owner, this, eventname);
        /*Dictionary<CharacterStats, float> moveSpeedDebuff = new Dictionary<CharacterStats, float>();
        moveSpeedDebuff.Add(CharacterStats.MoveSpeed, -99);
        owner.AddBuff("debuff_ms_attack", moveSpeedDebuff, false, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);*/
    }

    IEnumerator HitCheck(float normalizedtime, Vector2 localPos, Vector2 area, string eventname) {
        yield return new WaitWhile(() => owner.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime < normalizedtime);
        if (owner.GetState() != CharacterStates.Attack)
            yield break;
        List<Character> closeEnemies = CharacterManager.instance.GetEnemies(GetOwner().GetTeam()).FindAll
            (c => Helper.IsInBox((Vector2)c.transform.position + c.GetComponent<BoxCollider2D>().offset - c.GetComponent<BoxCollider2D>().size / 2f, (Vector2)c.transform.position + c.GetComponent<BoxCollider2D>().offset + c.GetComponent<BoxCollider2D>().size / 2f, (Vector2)owner.transform.position + localPos - area / 2f, (Vector2)owner.transform.position + localPos + area / 2f));
        StartCoroutine(Helper.DrawBox((Vector2)owner.transform.position + localPos, area, Color.red));
        Vector2 avgHitPos = new Vector2();
        List<Character> actualEnemiesHit = new List<Character>();
        foreach (Character c in closeEnemies) {
            if (c.HasFlag(CharacterFlags.Invincible)) {
                continue;
            }
            if (Helper.IsBlockedByMap(transform.position, c.transform.position))
                continue;
            Vector2 hitpos = Helper.SnapToBox(c.transform.position, c.GetCollider(), (Vector2)owner.transform.position + localPos);
            actualEnemiesHit.Add(c);
            avgHitPos.x += hitpos.x / closeEnemies.Count;
            avgHitPos.y += hitpos.y / closeEnemies.Count;
            OnWeaponHit(c, hitpos, eventname);
        }
        if(actualEnemiesHit.Count > 0) {
            if (GameDataManager.instance.GetData(className, "Sprites", "hit") != null)
                EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData(className, "Sprites", "hit"), avgHitPos, owner.GetFacingDirection());
        }
    }

    IEnumerator DelayedCharge(float normalizedtime, float chargeAmount) {
        yield return new WaitWhile(() => owner.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime < normalizedtime);
        if (owner.GetState() != CharacterStates.Attack)
            yield break;
        owner.GetController().ForceMove(0);
        owner.AddForce(Vector2.right * chargeAmount * owner.GetFacingDirection());
    }

    public virtual void OnWeaponHit(Character victim, Vector2 hitPos, string eventname) {
        if(owner == CharacterManager.instance.GetPlayer() || victim == CharacterManager.instance.GetPlayer())
            CamController.instance.ShakeCam(Mathf.Clamp(owner.GetCurrentStat(this, WeaponStats.Damage) / 20f, 0, 2),
                Mathf.Clamp(owner.GetCurrentStat(this, WeaponStats.Damage) / 50f, 0, 0.5f));
        if (victim.HasFlag(CharacterFlags.Invincible))
            return;
        DamageData dmgData = Helper.DamageCalc(owner, GetEssentialStats(), victim);
        victim.DoDamage(owner, dmgData.damage, dmgData.stagger);
        if (!victim.HasFlag(CharacterFlags.KnockBackImmunity)) {
            Vector2 knockback = new Vector2();
            bool knockout = false;
            if (GameDataManager.instance.GetData(eventname, "KnockBack") != null) {
                knockback.x = Convert.ToSingle(GameDataManager.instance.GetData(eventname, "KnockBack", "X")) * owner.GetFacingDirection();
                knockback.y = Convert.ToSingle(GameDataManager.instance.GetData(eventname, "KnockBack", "Y"));
                knockout = true;
            }
            else if (GameDataManager.instance.GetData(eventname, "ChargeAmount") != null) {
                knockback.x = Convert.ToSingle(GameDataManager.instance.GetData(eventname, "ChargeAmount")) * owner.GetFacingDirection();
                knockout = true;
            }
            victim.AddForce(knockback, knockout);
        }
    }

    public virtual void OnWeaponEvent(string eventname) {
        if (EventManager.Event_WeaponEvent != null)
            EventManager.Event_WeaponEvent(owner, this, eventname);
    }
}