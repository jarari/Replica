using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Bullet {
    protected float falloff50;
    protected float falloff25;
    protected Vector3 velVector;

    public void Initialize(string classname, Character user, Weapon firedfrom, float _speed, float _range, Dictionary<WeaponStats, float> _data, bool candirecthit) {
        className = classname;
		bulletData = GameDataManager.instance.RootData[className];

        attacker = user;
        weapon = firedfrom;
        speed = _speed;
        range = _range;
        data = _data;
        startPos = transform.position;

		if(bulletData["Sprites"]["smoke"])
            EffectManager.CreateEffect(
				bulletData["Sprites"]["smoke"].Value<string>(), 
				transform.position, 
				transform.eulerAngles.z, 
				transform
				);

		if (GameDataManager.instance.GetAnimatorController(classname))
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else {
            transform.localScale = new Vector3(0, 0, 0);
            anim = null;
        }

        if (bulletData["PhysicsMaterial"])
			rb.sharedMaterial = Helper.GetPhysicsMaterial2D(
				"Sprites/tileset/physicsmat",
				bulletData["PhysicsMaterial"].Value<string>()
				);

        if (bulletData["FallOff50"])
            falloff50 = range * bulletData["FallOff50"].Value<float>();
        else 
            falloff50 = range * 0.5f;

        if (bulletData["FallOff25"])
            falloff25 = range * bulletData["FallOff25"].Value<float>();
        else
            falloff25 = range * 0.75f;

        rb.velocity = transform.right * speed;
        velVector = transform.right;
        init = true;
        AdditionalData();
        if (candirecthit) {
            Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], user.GetComponents<Collider2D>()[0]);
            Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], user.GetComponents<Collider2D>()[1]);
            gameObject.layer = LayerMask.NameToLayer("Bullet");
        }
    }

    /*private void Update() {
        Collider2D[] colliding = Physics2D.OverlapBoxAll(transform.position, new Vector2(128, 128), 0, layer);
        foreach (Collider2D collider in colliding) {
            if (collider.GetComponent<Character>() != null && collider.GetComponent<Character>().GetTeam() == attacker.GetTeam()) {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.GetComponents<Collider2D>()[0]);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.GetComponents<Collider2D>()[1]);
            }
        }
    }*/

    protected virtual void AdditionalData() {

    }

    protected override void FixedUpdate() {
        if (!init) return;
        rb.velocity += new Vector2(0, Physics2D.gravity.y * Time.fixedDeltaTime);
        velVector = rb.velocity.normalized;
        float ang = Helper.Vector2ToAng(rb.velocity);
        if (ang >= 90 && ang <= 270) {
            GetComponent<SpriteRenderer>().flipY = true;
        }
        else {
            GetComponent<SpriteRenderer>().flipY = false;
        }
        Vector3 proang = transform.eulerAngles;
        proang.z = ang;
        transform.eulerAngles = proang;
        Collider2D[] colliding = Physics2D.OverlapBoxAll(transform.position, new Vector2(128, 128), 0, Helper.characterLayer);
        foreach (Collider2D collider in colliding) {
            if (collider.GetComponent<Character>() != null && collider.GetComponent<Character>().GetTeam() == attacker.GetTeam()) {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.GetComponents<Collider2D>()[0]);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.GetComponents<Collider2D>()[1]);
            }
        }
    }

    protected override void HandleCollision(Collider2D collider) {
        if (collider.gameObject == null) return;
        if (collider.gameObject.tag.Equals("Character")) {
            if(collider.gameObject.GetComponent<Character>().GetTeam() != attacker.GetTeam()) {
                collided = true;
                PoolDestroy();
                init = false;
                return;
            }
            else {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.gameObject.GetComponents<Collider2D>()[0]);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.gameObject.GetComponents<Collider2D>()[1]);
                return;
            }
        }
        else if (collider.gameObject.tag.Equals("Ground") || collider.gameObject.tag.Equals("Ceiling")) {
            collided = true;
            PoolDestroy();
            init = false;
            return;
        }
    }

    protected virtual void HitEffect() {
        if (anim && bulletData["Sprites"]["hit"]) {
            Vector3 temp = collisionNorm;
            temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
            float ang = Helper.Vector2ToAng(temp);
            EffectManager.CreateEffect(
				bulletData["Sprites"]["hit"].Value<string>(), 
				collisionPos, 
				ang
				);
        }
    }

    protected virtual void Explode() {
        List<Character> closeCharacters = CharacterManager.GetAllCharacters().FindAll(c => Vector3.Distance(Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), transform.position), transform.position) <= range);
        foreach (Character c in closeCharacters) {
            if (c.HasFlag(CharacterFlags.Invincible))
                continue;

            Vector2 cBoxBorder = Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), transform.position);
            if (Helper.IsBlockedByMap(transform.position, c.transform.position))
                continue;

            DamageData dmgdata = Helper.DamageCalc(attacker, data, c, true, true);
            float dist = Vector3.Distance(cBoxBorder, transform.position);
            float mult = 1;
            if (dist > falloff50) {
                if (dist <= falloff25)
                    mult = 0.5f;
                else
                    mult = 0.25f;
            }
            c.DoDamage(attacker, dmgdata.damage * mult, dmgdata.stagger);

            if (bulletData["KnockBack"] && !c.HasFlag(CharacterFlags.KnockBackImmunity)) {
                c.AddForce(
					(c.transform.position - transform.position).normalized * bulletData["KnockBack"].Value<float>() * mult, 
					true
					);
            }
        }
    }

    protected override void PoolDestroy() {
        ShakeCam();
        Explode();
        HitEffect();
        BulletManager.OnProjectileDestroy(this);
    }
}
