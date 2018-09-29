using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* 수류탄 클래스
 * 수류탄은 시간이 지나면 터지고
 * 처음 충돌이 일어난 순간부터는 캐릭터와 충돌하지 않음 (캐릭터 발에 치이는 문제 방지) */
class ThrowableGrenade : Throwable{
    private float cookTime = 0;
    private bool flying = false;
    protected override void AdditionalData() {
        flying = true;
        cookTime = Convert.ToSingle(GameDataManager.instance.GetData(className, "CookTime"));
    }

    protected override void FixedUpdate() {
        if (!init) return;
        cookTime -= Time.fixedDeltaTime;
        if(cookTime <= 0) {
            DestroyObject(gameObject);
            init = false;
        }
        if (flying) {
            rb.velocity += new Vector2(0, -981f * Time.fixedDeltaTime);
            Vector3 temp = transform.right;
            temp = Quaternion.AngleAxis(torque * Time.fixedDeltaTime, Vector3.forward) * temp;
            float ang = Helper.Vector2ToAng(temp);
            /*if (ang >= 90 && ang <= 270) {
                GetComponent<SpriteRenderer>().flipY = true;
            }
            else {
                GetComponent<SpriteRenderer>().flipY = false;
            }*/
            Vector3 proang = transform.eulerAngles;
            proang.z = ang;
            transform.eulerAngles = proang;
        }
    }

    protected override void HandleCollision(Collision2D collision) {
        if (flying) {
            flying = false;
            rb.gravityScale = 1;
            gameObject.layer = LayerMask.NameToLayer("Projectile");
        }
        return;
    }

    protected override void HitEffect() {
		base.HitEffect();{
			ParticleManager.instance.CreateParticle ("particle_he_explosion", transform.position, 0, false);
			ParticleManager.instance.CreateParticle ("particle_he_explosionlight", transform.position, 0, false);
		}
    }
}
