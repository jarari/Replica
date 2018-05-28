using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ThrowableGrenade : Throwable{
    private float cookTime = 0;
    private bool flying = false;
    protected override void AdditionalData() {
        flying = true;
        cookTime = Convert.ToSingle(GameDataManager.instance.GetData("Data", className, "CookTime"));
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
}
