using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 총알 클래스
 * 총알은 생성 후 중력과 관계없이 일직선으로 진행
 * 충돌시 사라지며 이펙트 생성 (json 데이터에 hit 컨트롤러 정의)
 * 캐릭터에게 데미지
 * 무적 상태의 캐릭터는 통과함 */
public class Bullet : MonoBehaviour {
    protected Character attacker;
    protected Weapon weapon;
    protected float speed;
    protected float range;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Vector3 collisionPos;
    protected Vector3 collisionNorm;
    protected Vector3 startPos;
    protected string className;
    protected bool collided = false;
    protected bool init = false;
    private bool ignoreGround = false;
    protected Dictionary<WeaponStats, float> data;

    protected void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        collided = false;
    }
    public void Initialize(string classname, Character user, Weapon firedfrom, Dictionary<WeaponStats, float> _data, bool ignoreground) {
        className = classname;
        attacker = user;
        weapon = firedfrom;
        speed = _data[WeaponStats.BulletSpeed];
        range = _data[WeaponStats.Range];
        data = _data;
        ignoreGround = ignoreground;
        startPos = transform.position;
        if (GameDataManager.instance.GetAnimatorController(classname) != null)
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else {
            transform.localScale = new Vector3(0, 0, 0);
            anim = null;
        }
            
        init = true;
    }

    /*private void Update() {
        if (!init) return;
        RaycastHit2D hit = Physics2D.Raycast(transform.position - transform.right * 0.06f, transform.right, 0.01f, layer);
        if (hit.collider != null) {
            collisionPos = transform.position + transform.right * 0.1f;
            HandleCollision(hit.collider);
        }
    }*/

    protected virtual void FixedUpdate() {
        if (!init) return;
        rb.velocity = transform.right * speed;
        if (Vector3.Distance(transform.position, startPos) >= range)
            DestroyObject(gameObject);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) {
        if (!init) return;
        collisionPos = collision.contacts[0].point;
        collisionNorm = collision.contacts[0].normal;
        HandleCollision(collision);
    }

    protected virtual void HandleCollision(Collision2D collision) {
        Character colliding = null;
        if(collision.gameObject == null) return;
        if (collision.gameObject.tag.Equals("Character")) {
            if (collision.gameObject.GetComponent<Character>().GetTeam() != attacker.GetTeam()) {
                colliding = collision.gameObject.GetComponent<Character>();
            }
            else {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collision.collider);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[1], collision.collider);
                return;
            }
        }
        else if (collision.gameObject.tag.Equals("Ground")) {
            if (ignoreGround) {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collision.collider);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[1], collision.collider);
            }
            else {
                collided = true;
                DestroyObject(gameObject);
                init = false;
                return;
            }
        }
        else if (collision.gameObject.tag.Equals("Ceiling")) {
            collided = true;
            DestroyObject(gameObject);
            init = false;
            return;
        }

        if (colliding != null) {
            if (colliding.GetTeam() != attacker.GetTeam()) {
                collided = true;
                if (colliding.HasFlag(CharacterFlags.Invincible)) {
                    Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collision.collider);
                    Physics2D.IgnoreCollision(GetComponents<Collider2D>()[1], collision.collider);
                }
                else {
                    Vector2 knockback = new Vector2();
                    if (GameDataManager.instance.GetData(className, "KnockBack") != null) {
                        knockback.x = Convert.ToSingle(GameDataManager.instance.GetData(className, "KnockBack", "X"));
                        knockback.y = Convert.ToSingle(GameDataManager.instance.GetData(className, "KnockBack", "Y"));
                    }
                    if (data[WeaponStats.ExplosionRadius] <= 0) {
                        DamageData dmgdata = Helper.DamageCalc(attacker, data, colliding, true, false);
                        colliding.DoDamage(attacker, dmgdata.damage, dmgdata.stagger);
                        if (GameDataManager.instance.GetData(className, "KnockBack") != null && !colliding.HasFlag(CharacterFlags.KnockBackImmunity)) {
                            colliding.AddForce(new Vector2(knockback.x * Mathf.Sign(colliding.transform.position.x - transform.position.x), knockback.y), true);
                        }
                    }
                    else {
                        List<Character> closeEnemies = CharacterManager.instance.GetEnemies(attacker.GetTeam()).FindAll(c => Vector3.Distance(Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), transform.position), transform.position) <= data[WeaponStats.ExplosionRadius]);
                        foreach (Character c in closeEnemies) {
                            if (c.HasFlag(CharacterFlags.Invincible))
                                continue;
                            Vector2 cBoxBorder = Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), transform.position);
                            if (Helper.IsBlockedByMap(transform.position, c.transform.position))
                                continue;
                            DamageData dmgdata = Helper.DamageCalc(attacker, data, c, true, true);
                            c.DoDamage(attacker, dmgdata.damage, dmgdata.stagger);
                            if (GameDataManager.instance.GetData(className, "KnockBack") != null && !c.HasFlag(CharacterFlags.KnockBackImmunity)) {
                                c.AddForce(new Vector2(knockback.x * Mathf.Sign(colliding.transform.position.x - transform.position.x), knockback.y), true);
                            }
                        }
                    }
                    DestroyObject(gameObject);
                    //EffectManager.instance.CreateEffect("effect_hitback_bullet", colliding.transform.position + transform.right * 10f, Helper.Vector2ToAng(transform.right));
                    init = false;
                    return;
                }
            }
        }
        
    }

    protected virtual void ShakeCam() {
        if (GameDataManager.instance.GetData(className, "ShakeCam") != null) {
            if (Vector2.Distance(CharacterManager.instance.GetPlayer().transform.position, transform.position) < Convert.ToSingle(GameDataManager.instance.GetData(className, "ShakeCam", "Radius"))) {
                CamController.instance.ShakeCam(Convert.ToSingle(GameDataManager.instance.GetData(className, "ShakeCam", "Magnitude"))
                , Convert.ToSingle(GameDataManager.instance.GetData(className, "ShakeCam", "Duration")));
            }
        }
    }

    protected virtual void OnDestroy() {
        if (!collided) return;
        ShakeCam();
        BulletManager.instance.OnBulletHit(this);
        if(anim != null && GameDataManager.instance.GetData(className, "Sprites", "hit") != null) {
            Vector3 temp = collisionNorm;
            temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
            float ang = Helper.Vector2ToAng(temp);
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData(className, "Sprites", "hit"), collisionPos, ang);
        }
    }
}
