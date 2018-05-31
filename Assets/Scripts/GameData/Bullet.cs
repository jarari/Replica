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
    private Character attacker;
    private Weapon weapon;
    private float speed;
    private float range;
    private Animator anim;
    private Rigidbody2D rb;
    private Vector3 collisionPos;
    private Vector3 collisionNorm;
    private Vector3 startPos;
    private string className;
    private bool collided = false;
    private bool init = false;
    private bool ignoreGround = false;
    private Dictionary<WeaponStats, float> data;

    private void Awake() {
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

    private void FixedUpdate() {
        if (!init) return;
        rb.velocity = transform.right * speed;
        if (Vector3.Distance(transform.position, startPos) >= range)
            DestroyObject(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!init) return;
        collisionPos = collision.contacts[0].point;
        collisionNorm = collision.contacts[0].normal;
        HandleCollision(collision);
    }

    private void HandleCollision(Collision2D collision) {
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
                    DamageData dmgdata = Helper.DamageCalc(attacker, data, colliding, true, false);
                    colliding.DoDamage(attacker, dmgdata.damage, dmgdata.stagger);
                    if (GameDataManager.instance.GetData("Data", className, "KnockBack") != null && !colliding.HasFlag(CharacterFlags.KnockBackImmunity)) {
                        Vector2 knockback = new Vector2();
                        knockback.x = Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "KnockBack", "X"));
                        knockback.y = Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "KnockBack", "Y"));
                        colliding.AddForce(new Vector2(transform.right.x * knockback.x, transform.right.y * knockback.y), true);
                    }
                    DestroyObject(gameObject);
                    //EffectManager.instance.CreateEffect("effect_hitback_bullet", colliding.transform.position + transform.right * 10f, Helper.Vector2ToAng(transform.right));
                    init = false;
                    return;
                }
            }
        }
        
    }

    private void OnDestroy() {
        if (!collided) return;
        if (GameDataManager.instance.GetData("Data", className, "ShakeCam") != null) {
            if (Vector2.Distance(CharacterManager.instance.GetPlayer().transform.position, transform.position) < Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "ShakeCam", "Radius"))) {
                CamController.instance.ShakeCam(Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "ShakeCam", "Magnitude"))
                , Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "ShakeCam", "Duration")));
            }
        }
        BulletManager.instance.OnBulletHit(this);
        if(anim != null && GameDataManager.instance.GetData("Data", className, "Sprites", "hit") != null) {
            Vector3 temp = collisionNorm;
            temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
            float ang = Helper.Vector2ToAng(temp);
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "hit"), collisionPos, ang);
        }
    }
}
