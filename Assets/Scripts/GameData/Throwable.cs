using System.Collections.Generic;
using UnityEngine;

public class Throwable : Projectile {
    protected float torque;
    public void Initialize(string classname, Character user, Weapon firedfrom, float _torque, Dictionary<WeaponStats, float> _data, bool candirecthit) {
        className = classname;
        attacker = user;
        weapon = firedfrom;
        speed = _data[WeaponStats.BulletSpeed];
        range = _data[WeaponStats.Range];
        torque = _torque;
        data = _data;
        startPos = transform.position;
        string smoke = (string)GameDataManager.instance.GetData("Data", className, "Sprites", "smoke");
        if (smoke != null && smoke != "")
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "smoke"), transform.position, transform.eulerAngles.z, transform);
        if (GameDataManager.instance.GetAnimatorController(classname) != null)
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else {
            transform.localScale = new Vector3(0, 0, 0);
            anim = null;
        }
        rb.velocity = transform.right * speed;
        init = true;
        AdditionalData();
        if (candirecthit)
            StartCoroutine(DirectHit());
    }

    protected override void FixedUpdate() {
        if (!init) return;
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
