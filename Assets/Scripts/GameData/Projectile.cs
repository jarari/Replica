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
        attacker = user;
        weapon = firedfrom;
        speed = _speed;
        range = _range;
        data = _data;
        startPos = transform.position;
        if(GameDataManager.instance.GetData(className, "Sprites", "smoke") != null)
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData(className, "Sprites", "smoke"), transform.position, transform.eulerAngles.z, transform);
        if (GameDataManager.instance.GetAnimatorController(classname) != null)
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else {
            transform.localScale = new Vector3(0, 0, 0);
            anim = null;
        }
        if (GameDataManager.instance.GetData(className, "PhysicsMaterial") != null) {
            rb.sharedMaterial = Helper.GetPhysicsMaterial2D("Sprites/tileset/physicsmat", (string)GameDataManager.instance.GetData(className, "PhysicsMaterial"));
        }
        if (GameDataManager.instance.GetData(className, "FallOff50") != null) {
            falloff50 = range * Convert.ToSingle(GameDataManager.instance.GetData(className, "FallOff50"));
        }
        else {
            falloff50 = range * 0.5f;
        }
        if (GameDataManager.instance.GetData(className, "FallOff25") != null) {
            falloff25 = range * Convert.ToSingle(GameDataManager.instance.GetData(className, "FallOff25"));
        }
        else {
            falloff25 = range * 0.75f;
        }
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

    protected override void HandleCollision(Collision2D collision) {
        if (collision.gameObject == null) return;
        if (collision.gameObject.tag.Equals("Character")) {
            if(collision.gameObject.GetComponent<Character>().GetTeam() != attacker.GetTeam()) {
                collided = true;
                DestroyObject(gameObject);
                init = false;
                return;
            }
            else {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collision.gameObject.GetComponents<Collider2D>()[0]);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collision.gameObject.GetComponents<Collider2D>()[1]);
                return;
            }
        }
        else if (collision.gameObject.tag.Equals("Ground") || collision.gameObject.tag.Equals("Ceiling")) {
            collided = true;
            DestroyObject(gameObject);
            init = false;
            return;
        }
    }

    protected virtual void HitEffect() {
        if (anim != null && GameDataManager.instance.GetData(className, "Sprites", "hit") != null) {
            Vector3 temp = collisionNorm;
            temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
            float ang = Helper.Vector2ToAng(temp);
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData(className, "Sprites", "hit"), collisionPos, ang);
        }
    }

    protected virtual void Explode() {
        List<Character> closeCharacters = CharacterManager.instance.GetAllCharacters().FindAll(c => Vector3.Distance(Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), transform.position), transform.position) <= range);
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
            if (GameDataManager.instance.GetData(className, "KnockBack") != null && !c.HasFlag(CharacterFlags.KnockBackImmunity)) {
                c.AddForce((c.transform.position - transform.position).normalized * Convert.ToSingle(GameDataManager.instance.GetData(className, "KnockBack")) * mult, true);
            }
        }
    }

    protected override void OnDestroy() {
        ShakeCam();
        Explode();
        HitEffect();
        BulletManager.instance.OnProjectileHit(this);
    }
}
