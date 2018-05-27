using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour {
    public static BulletManager instance;
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public Bullet CreateBullet(string classname, Vector3 pos, Character user, Weapon firedfrom, float angle, Dictionary<WeaponStats, float> _data) {
        GameObject bullet_obj = (GameObject)Instantiate(Resources.Load("Prefab/Bullet"), pos, new Quaternion());
        Bullet bullet;
        if (GameDataManager.instance.GetData("Data", classname, "ScriptClass") != null && (string)GameDataManager.instance.GetData("Data", classname, "ScriptClass") != "") {
            bullet = (Bullet)bullet_obj.gameObject.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", classname, "ScriptClass")));
        }
        else {
            bullet = bullet_obj.gameObject.AddComponent<Bullet>();
        }
        Physics2D.IgnoreCollision(bullet_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[0]);
        Physics2D.IgnoreCollision(bullet_obj.GetComponents<Collider2D>()[1], user.GetComponentsInParent<Collider2D>()[0]);
        Physics2D.IgnoreCollision(bullet_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[1]);
        Physics2D.IgnoreCollision(bullet_obj.GetComponents<Collider2D>()[1], user.GetComponentsInParent<Collider2D>()[1]);
        Vector3 ang = bullet_obj.transform.eulerAngles;
        ang.z = angle;
        bullet_obj.transform.eulerAngles = ang;
        bullet.Initialize(classname, user, firedfrom, _data, false);
        return bullet;
    }

    public Bullet CreateBullet(string classname, Vector3 pos, Character user, Weapon firedfrom, float angle, Dictionary<WeaponStats, float> _data, bool ignoreGround) {
        GameObject bullet_obj = (GameObject)Instantiate(Resources.Load("Prefab/Bullet"), pos, new Quaternion());
        Bullet bullet;
        if (GameDataManager.instance.GetData("Data", classname, "ScriptClass") != null && (string)GameDataManager.instance.GetData("Data", classname, "ScriptClass") != "") {
            bullet = (Bullet)bullet_obj.gameObject.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", classname, "ScriptClass")));
        }
        else {
            bullet = bullet_obj.gameObject.AddComponent<Bullet>();
        }
        Physics2D.IgnoreCollision(bullet_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[0]);
        Physics2D.IgnoreCollision(bullet_obj.GetComponents<Collider2D>()[1], user.GetComponentsInParent<Collider2D>()[0]);
        Physics2D.IgnoreCollision(bullet_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[1]);
        Physics2D.IgnoreCollision(bullet_obj.GetComponents<Collider2D>()[1], user.GetComponentsInParent<Collider2D>()[1]);
        Vector3 ang = bullet_obj.transform.eulerAngles;
        ang.z = angle;
        bullet_obj.transform.eulerAngles = ang;
        bullet.Initialize(classname, user, firedfrom, _data, ignoreGround);
        return bullet;
    }

    public Projectile CreateProjectile(string classname, Vector3 pos, Character user, Weapon firedfrom, float angle, Dictionary<WeaponStats, float> _data, bool candirecthit) {
        GameObject projectile_obj = (GameObject)Instantiate(Resources.Load("Prefab/Projectile"), pos, new Quaternion());
        Projectile projectile;
        if (GameDataManager.instance.GetData("Data", classname, "ScriptClass") != null && (string)GameDataManager.instance.GetData("Data", classname, "ScriptClass") != "") {
            projectile = (Projectile)projectile_obj.gameObject.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", classname, "ScriptClass")));
        }
        else {
            projectile = projectile_obj.gameObject.AddComponent<Projectile>();
        }
        Physics2D.IgnoreCollision(projectile_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[0]);
        Physics2D.IgnoreCollision(projectile_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[1]);
        Vector3 ang = projectile_obj.transform.eulerAngles;
        ang.z = angle;
        projectile_obj.transform.eulerAngles = ang;
        projectile.Initialize(classname, user, firedfrom, _data, candirecthit);
        return projectile;
    }

    public Throwable CreateThrowable(string classname, Vector3 pos, Character user, Weapon firedfrom, float angle, float torque, Dictionary<WeaponStats, float> _data, bool candirecthit) {
        GameObject throwable_obj = (GameObject)Instantiate(Resources.Load("Prefab/Projectile"), pos, new Quaternion());
        Throwable throwable;
        if (GameDataManager.instance.GetData("Data", classname, "ScriptClass") != null && (string)GameDataManager.instance.GetData("Data", classname, "ScriptClass") != "") {
            throwable = (Throwable)throwable_obj.gameObject.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", classname, "ScriptClass")));
        }
        else {
            throwable = throwable_obj.gameObject.AddComponent<Throwable>();
        }
        Physics2D.IgnoreCollision(throwable_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[0]);
        Physics2D.IgnoreCollision(throwable_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[1]);
        Vector3 ang = throwable_obj.transform.eulerAngles;
        ang.z = angle;
        throwable_obj.transform.eulerAngles = ang;
        throwable.Initialize(classname, user, firedfrom, torque, _data, candirecthit);
        return throwable;
    }
}
