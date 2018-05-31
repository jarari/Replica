using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    protected Character attacker;
    protected Weapon weapon;
    protected float speed;
    protected float range;
    protected float falloff50;
    protected float falloff25;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Vector3 collisionPos;
    protected Vector3 collisionNorm;
    protected Vector3 startPos;
    protected Vector3 velVector;
    protected string className;
    protected bool collided = false;
    protected bool init = false;
    protected Dictionary<WeaponStats, float> data;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        collided = false;
    }
    public void Initialize(string classname, Character user, Weapon firedfrom, float _speed, float _range, Dictionary<WeaponStats, float> _data, bool candirecthit) {
        className = classname;
        attacker = user;
        weapon = firedfrom;
        speed = _speed;
        range = _range;
        data = _data;
        startPos = transform.position;
        if(GameDataManager.instance.GetData("Data", className, "Sprites", "smoke") != null)
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "smoke"), transform.position, transform.eulerAngles.z, transform);
        if (GameDataManager.instance.GetAnimatorController(classname) != null)
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else {
            transform.localScale = new Vector3(0, 0, 0);
            anim = null;
        }
        if (GameDataManager.instance.GetData("Data", className, "PhysicsMaterial") != null) {
            rb.sharedMaterial = Helper.GetPhysicsMaterial2D((string)GameDataManager.instance.GetData("Data", className, "PhysicsMaterial"));
        }
        if (GameDataManager.instance.GetData("Data", className, "FallOff50") != null) {
            falloff50 = range * Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "FallOff50"));
        }
        else {
            falloff50 = range * 0.5f;
        }
        if (GameDataManager.instance.GetData("Data", className, "FallOff25") != null) {
            falloff25 = range * Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "FallOff25"));
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

    protected virtual void FixedUpdate() {
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

    private void OnCollisionEnter2D(Collision2D collision) {
        collisionPos = collision.contacts[0].point;
        collisionNorm = collision.contacts[0].normal;
        HandleCollision(collision);
    }

    protected virtual void HandleCollision(Collision2D collision) {
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

    protected virtual void ExplosionEffect() {
        if (anim != null && GameDataManager.instance.GetData("Data", className, "Sprites", "hit") != null) {
            Vector3 temp = collisionNorm;
            temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
            float ang = Helper.Vector2ToAng(temp);
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "hit"), collisionPos, ang);
        }
    }

    protected virtual void Explode() {
        List<Character> closeEnemies = CharacterManager.instance.GetAllCharacters().FindAll(c => Vector3.Distance(Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), transform.position), transform.position) <= range);
        foreach (Character c in closeEnemies) {
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
            if (GameDataManager.instance.GetData("Data", className, "KnockBack") != null && !c.HasFlag(CharacterFlags.KnockBackImmunity)) {
                c.AddForce((c.transform.position - transform.position).normalized * Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "KnockBack")) * mult, true);
            }
        }
    }

    protected void OnDestroy() {
        if (GameDataManager.instance.GetData("Data", className, "ShakeCam") != null) {
            if(Vector2.Distance(CharacterManager.instance.GetPlayer().transform.position, transform.position) < Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "ShakeCam", "Radius"))){
                CamController.instance.ShakeCam(Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "ShakeCam", "Magnitude"))
                , Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "ShakeCam", "Duration")));
            }
        }
        Explode();
        ExplosionEffect();
        BulletManager.instance.OnProjectileHit(this);
    }
}
