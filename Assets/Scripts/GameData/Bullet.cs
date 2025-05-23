﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 총알 클래스
 * 총알은 생성 후 중력과 관계없이 일직선으로 진행
 * 충돌시 사라지며 이펙트 생성 (json 데이터에 hit 컨트롤러 정의)
 * 캐릭터에게 데미지
 * 무적 상태의 캐릭터는 통과함 */
public class Bullet : MonoBehaviour {
    const float lifetime = 3f;
    protected Character attacker;
    protected Weapon weapon;
    protected float speed;
    protected float range;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected CircleCollider2D cc;
    protected Vector3 collisionPos;
    protected Vector3 collisionNorm;
    protected Vector3 startPos;
    protected string className;
    protected bool collided = false;
    protected bool init = false;
    private bool ignoreGround = false;
    private float wasAliveFor = 0f;
    protected Dictionary<WeaponStats, float> data;

	protected JDictionary bulletData;

    protected void Awake() {
		rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cc = GetComponent<CircleCollider2D>();
        collided = false;
    }

    public void Initialize(string classname, Character user, Weapon firedfrom, Dictionary<WeaponStats, float> _data, bool ignoreground) {
        className = classname;
		bulletData = GameDataManager.instance.RootData[className];

		attacker = user;
        weapon = firedfrom;
        speed = _data[WeaponStats.BulletSpeed];
        range = _data[WeaponStats.Range];
        data = _data;
        ignoreGround = ignoreground;
        startPos = transform.position;

		if (GameDataManager.instance.GetAnimatorController(classname))
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else {
            transform.localScale = new Vector3(0, 0, 0);
            anim = null;
        }

        cc.offset = new Vector2(35, 0);

        wasAliveFor = 0f;

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
        wasAliveFor += Time.fixedDeltaTime;
        rb.velocity = transform.right * speed;
        RaycastHit2D hit = Physics2D.Raycast(rb.position, transform.right, speed * Time.fixedDeltaTime, Helper.characterLayer);
        if (hit.collider != null) {
            collisionPos = hit.point;
            collisionNorm = hit.normal;
            HandleCollision(hit.collider);
        }
        if (Vector3.Distance(transform.position, startPos) >= range || wasAliveFor >= lifetime)
            PoolDestroy();
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) {
        if (!init) return;
        collisionPos = collision.contacts[0].point;
        collisionNorm = collision.contacts[0].normal;
        HandleCollision(collision.collider);
    }

    protected virtual void HandleCollision(Collider2D collider) {
        Character colliding = null;
        if(collider.gameObject == null) return;
        if (collider.gameObject.tag.Equals("Character")) {
            if (collider.gameObject.GetComponent<Character>().GetTeam() != attacker.GetTeam()) {
                colliding = collider.gameObject.GetComponent<Character>();
            }
            else {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[1], collider);
                return;
            }
        }
        else if (collider.gameObject.tag.Equals("Ground")) {
            if (ignoreGround) {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[1], collider);
            }
            else {
                collided = true;
                PoolDestroy();
                init = false;
                return;
            }
        }
        else if (collider.gameObject.tag.Equals("Ceiling")) {
            collided = true;
            PoolDestroy();
            init = false;
            return;
        }

        if (colliding != null) {
            if (colliding.GetTeam() != attacker.GetTeam()) {
                collided = true;
                if (colliding.HasFlag(CharacterFlags.Invincible)) {
                    Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider);
                    Physics2D.IgnoreCollision(GetComponents<Collider2D>()[1], collider);
                }
                else {
                    if (data[WeaponStats.ExplosionRadius] <= 0) {
                        DamageData dmgdata = Helper.DamageCalc(attacker, data, colliding, true, false);
                        colliding.DoDamage(attacker, dmgdata.damage, dmgdata.stagger);

						// 함수?
                        if (bulletData["KnockBack"] && !colliding.HasFlag(CharacterFlags.KnockBackImmunity)) {
							Vector2 knockback;
							knockback = new Vector2(
								bulletData["KnockBack"]["X"].Value<float>(),
								bulletData["KnockBack"]["Y"].Value<float>()
							);
							Vector2 knockbackForce = new Vector2(
								knockback.x * Mathf.Sign(colliding.transform.position.x - transform.position.x), 
								knockback.y
								);
							colliding.AddForce(knockbackForce, true);
                        }
                    }
                    else {
                        List<Character> closeEnemies = 
							CharacterManager.GetEnemies(attacker.GetTeam()).FindAll(
								enemy => 
								Vector3.Distance(
									Helper.GetClosestBoxBorder(enemy.transform.position, enemy.GetComponent<BoxCollider2D>(), 
									transform.position), transform.position
									) < data[WeaponStats.ExplosionRadius]
								);

                        foreach (Character enemy in closeEnemies) {
                            if (enemy.HasFlag(CharacterFlags.Invincible))
                                continue;

                            if (Helper.IsBlockedByMap(transform.position, enemy.transform.position))
                                continue;

							DamageData dmgdata = Helper.DamageCalc(attacker, data, enemy, true, true);
							enemy.DoDamage(attacker, dmgdata.damage, dmgdata.stagger);

							// 함수?
                            if (bulletData["KnockBack"] && !enemy.HasFlag(CharacterFlags.KnockBackImmunity)) {
								Vector2 knockback;
								knockback = new Vector2(
									bulletData["KnockBack"]["X"].Value<float>(),
									bulletData["KnockBack"]["Y"].Value<float>()
								);
								Vector2 knockbackForce = new Vector2(
								knockback.x * Mathf.Sign(colliding.transform.position.x - transform.position.x),
								knockback.y
								);
								enemy.AddForce(knockbackForce, true);
							}
                        }
                    }

                    PoolDestroy();
                    //EffectManager.instance.CreateEffect("effect_hitback_bullet", colliding.transform.position + transform.right * 10f, Helper.Vector2ToAng(transform.right));
                    init = false;
                    return;
                }
            }
        }
        
    }

    protected virtual void ShakeCam() {
        if (bulletData["ShakeCam"]) {
			float distance = Vector2.Distance(CharacterManager.GetPlayer().transform.position, transform.position);
			float radius = bulletData["ShakeCam"]["Radius"].Value<float>();
			if (distance < radius) {
                CamController.instance.ShakeCam(
					bulletData["ShakeCam"]["Magnitude"].Value<float>(),
					bulletData["ShakeCam"]["Duration"].Value<float>()
					);
            }
        }
    }

    protected virtual void PoolDestroy() {
        BulletManager.OnBulletDestroy(this);
        if (!collided) return;
        ShakeCam();
        Vector3 temp = collisionNorm;
        temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
        float ang = Helper.Vector2ToAng(temp);
        if (anim && bulletData["Sprites"]["hit"]) {
            EffectManager.CreateEffect(bulletData["Sprites"]["hit"].Value<string>(), collisionPos, ang);
        }
        if (bulletData["Sprites"]["hitparticles"]) {
            //Dictionary<string, object> dict = (Dictionary<string, object>)GameDataManager.instance.GetData(className, "Sprites", "hitparticles");
            //for (int i = 0; i < dict.Count; i++) {
            //    string particleName = (string)dict[i.ToString()];
            //    ParticleManager.instance.CreateParticle(particleName, collisionPos, ang, false);
            //}
            foreach (JDictionary particle in bulletData["Sprites"]["hitparticles"]) {
                ParticleManager.instance.CreateParticle(particle.Value<string>(), collisionPos, ang, false);
            }
        }
    }
}
