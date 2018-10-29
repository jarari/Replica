using System.Collections.Generic;
using UnityEngine;

public class Throwable : Projectile {
    protected float torque;
    public void Initialize(string classname, Character user, Weapon firedfrom, float _speed, float _range, float _torque, Dictionary<WeaponStats, float> _data, bool candirecthit) {
        Initialize(classname, user, firedfrom, _speed, _range, _data, candirecthit);
        torque = _torque;
    }

    protected override void FixedUpdate() {
        if (!init) return;
        rb.velocity += new Vector2(0, -981.0f * Time.fixedDeltaTime);
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

    protected override void HitEffect() {
        if (anim && bulletData["Sprites"]["hit"]) {
            EffectManager.CreateEffect(
				bulletData["Sprites"]["hit"].Value<string>(), 
				collisionPos, 
				0
				);
        }
    }
}
