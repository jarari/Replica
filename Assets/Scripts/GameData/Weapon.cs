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

public class Weapon : ObjectBase {

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

    public void FireBullet(string bulletclass, float ang) {
        BulletManager.instance.CreateBullet(bulletclass, GetMuzzlePos(), owner, this, 90 - ang * owner.GetFacingDirection(), GetEssentialStats());
    }

    public virtual void OnAttack(string eventname) {
        if (!owner.IsOnGround()) {
            if(owner.GetComponent<Rigidbody2D>().velocity.y < 300)
                owner.GetController().SetVelY(300);
        }
        if(GameDataManager.instance.GetData("Data", eventname, "ChargeAmount") != null) {
            owner.GetController().ForceMove(0);
            owner.AddForce(Vector2.right * Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "ChargeAmount")) * owner.GetFacingDirection());
        }
        Dictionary<WeaponStats, float> dmgMult = new Dictionary<WeaponStats, float>();
        dmgMult.Add(WeaponStats.Damage, Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "DamageMultiplier")));
        WeaponBuff dmgMultBuff = new WeaponBuff(dmgMult, type);
        owner.AddWeaponBuff("buff_damage_" + eventname, dmgMultBuff, false, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);

        Dictionary<WeaponStats, float> ccAdd = new Dictionary<WeaponStats, float>();
        ccAdd.Add(WeaponStats.SADestruction, Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "SADestruction")));
        ccAdd.Add(WeaponStats.Stagger, Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "Stagger")));
        WeaponBuff ccAddBuff = new WeaponBuff(ccAdd, type);
        owner.AddWeaponBuff("buff_cc_" + eventname, ccAddBuff, true, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);
        
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
                StartCoroutine(HitCheck(time / numOfFrames, pos, area, eventname));
            }
        }
        /*Dictionary<CharacterStats, float> moveSpeedDebuff = new Dictionary<CharacterStats, float>();
        moveSpeedDebuff.Add(CharacterStats.MoveSpeed, -99);
        owner.AddBuff("debuff_ms_attack", moveSpeedDebuff, false, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);*/
    }

    IEnumerator HitCheck(float normalizedtime, Vector2 localPos, Vector2 area, string eventname) {
        yield return new WaitWhile(() => owner.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime < normalizedtime);
        List<Character> closeEnemies = CharacterManager.instance.GetEnemies(GetOwner().GetTeam()).FindAll
            (c => Helper.IsInBox((Vector2)c.transform.position + c.GetComponent<BoxCollider2D>().offset - c.GetComponent<BoxCollider2D>().size / 2f, (Vector2)c.transform.position + c.GetComponent<BoxCollider2D>().offset + c.GetComponent<BoxCollider2D>().size / 2f, (Vector2)owner.transform.position + localPos - area / 2f, (Vector2)owner.transform.position + localPos + area / 2f));
        StartCoroutine(Helper.DrawBox((Vector2)owner.transform.position + localPos, area, Color.red));
        foreach (Character c in closeEnemies) {
            float hitposX = Mathf.Clamp(((Vector2)owner.transform.position + localPos).x,
                c.transform.position.x + c.GetCollider().offset.x - c.GetCollider().size.x / 2f,
                c.transform.position.x + c.GetCollider().offset.x + c.GetCollider().size.x / 2f);
            float hitposY = Mathf.Clamp(((Vector2)owner.transform.position + localPos).y,
                c.transform.position.y + c.GetCollider().offset.y - c.GetCollider().size.y / 2f,
                c.transform.position.y + c.GetCollider().offset.y + c.GetCollider().size.y / 2f);
            OnWeaponHit(c, new Vector2(hitposX, hitposY), eventname);
        }
    }

    public virtual void OnWeaponHit(Character victim, Vector2 hitPos, string eventname) {
        CamController.instance.ShakeCam(Mathf.Clamp(owner.GetCurrentStat(this, WeaponStats.Damage) / 20f, 0, 2),
            Mathf.Clamp(owner.GetCurrentStat(this, WeaponStats.Damage) / 50f, 0, 0.5f));
        if(GameDataManager.instance.GetData("Data", className, "Sprites", "hit") != null)
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "hit"), hitPos, owner.GetFacingDirection());
        if (victim.HasFlag(CharacterFlags.Invincible))
            return;
        DamageData dmgData = Helper.DamageCalc(owner, GetEssentialStats(), victim);
        victim.DoDamage(owner, dmgData.damage, dmgData.stagger);
        if (!victim.HasFlag(CharacterFlags.KnockBackImmunity)) {
            Vector2 knockback = new Vector2();
            bool knockout = false;
            if (GameDataManager.instance.GetData("Data", eventname, "KnockBack") != null) {
                knockback.x = Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "KnockBack", "X")) * owner.GetFacingDirection();
                knockback.y = Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "KnockBack", "Y"));
                knockout = true;
            }
            else if (GameDataManager.instance.GetData("Data", eventname, "ChargeAmount") != null) {
                knockback.x = Convert.ToSingle(GameDataManager.instance.GetData("Data", eventname, "ChargeAmount")) * owner.GetFacingDirection();
                knockout = true;
            }
            victim.AddForce(knockback, knockout);
        }
    }

    public virtual void OnWeaponEvent(string eventname) { }
}