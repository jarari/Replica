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

        type = (WeaponTypes) objectData["Stats"]["WeaponType"].Value<int>();

        if (owner) {
            for (int i = 0; i < (int)WeaponStats.EndOfEnums; i++) {
                owner.SetBaseStat(this, (WeaponStats)i, owner.GetBaseStat(this, (WeaponStats)i));
            }
            GetComponent<SpriteRenderer>().sortingOrder = GetOwner().GetComponent<SpriteRenderer>().sortingOrder + 1;
        }

        muzzlePos = new Vector2(
			objectData["MuzzlePos"]["X"].Value<float>(),
			objectData["MuzzlePos"]["Y"].Value<float>()
			);

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
        if(string.IsNullOrEmpty(b)) {
            if(objectData["Stats"]["DefaultBullet"]) {
                bullet = objectData["Stats"]["DefaultBullet"].Value<string>();
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
		JDictionary bulletData = GameDataManager.instance.RootData[bulletclass];

        if (bulletData["Type"].Value<string>() == "bullet")
            BulletManager.instance.CreateBullet(bulletclass, GetMuzzlePos(), owner, this, 90 - ang * owner.GetFacingDirection(), GetEssentialStats());

        else if (bulletData["Type"].Value<string>() == "laser")
            BulletManager.instance.CreateLaser(bulletclass, GetMuzzlePos(), owner, this, 90 - ang * owner.GetFacingDirection(), GetEssentialStats());

        if (EventManager.Event_WeaponFire != null)
            EventManager.Event_WeaponFire(owner, this, bulletclass);
    }

    public void CreateEffect(string effectname) {
        EffectManager.instance.CreateEffect(effectname, transform.position, 0, null, !owner.IsFacingRight());
    }

    public virtual void OnAttack(string eventname) {
        if (!owner.IsOnGround()) {
            if(owner.GetComponent<Rigidbody2D>().velocity.y < 300)
                owner.GetController().SetVelY(300);
        }

		JDictionary attackData = GameDataManager.instance.RootData[eventname];

        if (attackData["Unstoppable"] && attackData["Unstoppable"].Value<int>() == 1) {
            owner.SetFlag(CharacterFlags.UnstoppableAttack);
        }

		float chargeAmount;
		if(attackData["ChargeAmount"]) {
			chargeAmount = attackData["ChargeAmount"].Value<float>();

			if(attackData["ChargeDelay"]) {
				float time = attackData["ChargeDelay"].Value<float>();
				AnimationClip clip = owner.GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip;
				float numOfFrames = Mathf.Round(clip.frameRate * clip.length);
				StartCoroutine(DelayedCharge(time / numOfFrames, chargeAmount));
			}
			else {
				owner.GetController().ForceMove(0);
				owner.AddForce(Vector2.right * chargeAmount * owner.GetFacingDirection());
			}
		}

        Dictionary<WeaponStats, float> dmgMult = new Dictionary<WeaponStats, float>();
        float mult = attackData["DamageMultiplier"].Value<float>() - 1.0f;
        if(mult != 0) {
            dmgMult.Add(WeaponStats.Damage, mult);
            WeaponBuff dmgMultBuff = new WeaponBuff(dmgMult, type);
            owner.AddWeaponBuff("buff_damage_" + eventname, dmgMultBuff, false, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);
        }

        Dictionary<WeaponStats, float> ccAdd = new Dictionary<WeaponStats, float>();
        ccAdd.Add(WeaponStats.SADestruction, attackData["SADestruction"].Value<float>());
        ccAdd.Add(WeaponStats.Stagger, attackData["Stagger"].Value<float>());
        if (attackData["ExplosionRadius"])
            ccAdd.Add(WeaponStats.ExplosionRadius, attackData["ExplosionRadius"].Value<float>());

        WeaponBuff ccAddBuff = new WeaponBuff(ccAdd, type);
        owner.AddWeaponBuff("buff_cc_" + eventname, ccAddBuff, true, owner.GetAnimator().GetCurrentAnimatorStateInfo(0).length);
        
        if(attackData["HitBox"] && attackData["HitBox"].Count > 0) {
			//int hitboxnum = ((Dictionary<string, object>)GameDataManager.instance.GetData(eventname, "HitBox")).Count;
			foreach(JDictionary hitBox in attackData["HitBox"]) {
				AnimationClip clip = owner.GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip;
				float numOfFrames = Mathf.Round(clip.frameRate * clip.length);
				float time = hitBox["Time"].Value<float>();
				Vector2 pos = new Vector2(
					hitBox["Pos"]["X"].Value<float>() * owner.GetFacingDirection(),
					hitBox["Pos"]["Y"].Value<float>()
					);
				Vector2 area = new Vector2(
					hitBox["Area"]["X"].Value<float>(),
					hitBox["Area"]["Y"].Value<float>()
					);
				StartCoroutine(HitCheck(time / numOfFrames, pos, area, eventname));
			}
			//for(int i = 0; i < hitboxnum; i++) {
			//	AnimationClip clip = owner.GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip;
			//	float time = Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Time"));
			//	float numOfFrames = Mathf.Round(clip.frameRate * clip.length);
			//	Vector2 pos = new Vector2(
			//		Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Pos", "X")) * owner.GetFacingDirection(),
			//		Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Pos", "Y"))
			//		);
			//	Vector2 area = new Vector2(
			//		Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Area", "X")),
			//		Convert.ToSingle(GameDataManager.instance.GetData(eventname, "HitBox", i.ToString(), "Area", "Y"))
			//		);
			//	StartCoroutine(HitCheck(time / numOfFrames, pos, area, eventname));
			//}
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
            if (objectData["Sprites"]["hit"])
                EffectManager.instance.CreateEffect(
					objectData["Sprites"]["hit"].Value<string>(), 
					avgHitPos, 
					0, 
					null, 
					!owner.IsFacingRight()
					);

            if(objectData["Sprites"]["hitparticles"]) {
				foreach(JDictionary particle in objectData["Sprites"]["hitparticles"]) {
					ParticleManager.instance.CreateParticle(particle.Value<string>(), avgHitPos, 0, !owner.IsFacingRight());
				}
                //Dictionary<string, object> dict = (Dictionary<string, object>)GameDataManager.instance.GetData(className, "Sprites", "hitparticles");
                //for(int i = 0; i < dict.Count; i++) {
                //    string particleName = (string)dict[i.ToString()];
                //    ParticleManager.instance.CreateParticle(particleName, avgHitPos, 0, !owner.IsFacingRight());
                //}
            }
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
            CamController.instance.ShakeCam(
				Mathf.Clamp(owner.GetCurrentStat(this, WeaponStats.Damage) / 20f, 0, 2),
                Mathf.Clamp(owner.GetCurrentStat(this, WeaponStats.Damage) / 50f, 0, 0.5f)
				);

        if (victim.HasFlag(CharacterFlags.Invincible))
            return;

        DamageData dmgData = Helper.DamageCalc(owner, GetEssentialStats(), victim);
        victim.DoDamage(owner, dmgData.damage, dmgData.stagger);

        if (!victim.HasFlag(CharacterFlags.KnockBackImmunity)) {
			JDictionary attackData = GameDataManager.instance.RootData[eventname];

			Vector2 knockback = new Vector2();
            bool knockout = false;

            if (attackData["KnockBack"]) {
                knockback.x = attackData["KnockBack"]["X"].Value<float>() * owner.GetFacingDirection();
                knockback.y = attackData["KnockBack"]["Y"].Value<float>();
                knockout = true;
            }
            else if (attackData["ChargeAmount"]) {
                knockback.x = attackData["ChargeAmount"].Value<float>() * owner.GetFacingDirection();
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