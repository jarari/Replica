﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 투사체 생성 클래스
 * 여기서는 총알, 포환, 투척물을 생성할 수 있음. */
public class BulletManager : MonoBehaviour {
    public static BulletManager instance;
    private List<Bullet> bullets = new List<Bullet>();
    private List<Projectile> projectiles = new List<Projectile>();
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public List<Bullet> GetBullets() {
        return bullets;
    }

    public void OnBulletHit(Bullet b) {
        bullets.Remove(b);
    }

    public List<Projectile> GetProjectiles() {
        return projectiles;
    }

    public void OnProjectileHit(Projectile p) {
        projectiles.Remove(p);
    }

    public Bullet CreateBullet(string classname, Vector3 pos, Character user, Weapon firedfrom, float angle, Dictionary<WeaponStats, float> _data, bool ignoreGround = false) {
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
        bullets.Add(bullet);
        return bullet;
    }

    public Projectile CreateProjectile(string classname, Vector3 pos, Character user, Weapon firedfrom, float speed, float range, float angle, Dictionary<WeaponStats, float> _data, bool candirecthit) {
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
        projectile.Initialize(classname, user, firedfrom, speed, range, _data, candirecthit);
        projectiles.Add(projectile);
        return projectile;
    }

    public Throwable CreateThrowable(string classname, Vector3 pos, Character user, Weapon firedfrom, float speed, float range, float angle, float torque, Dictionary<WeaponStats, float> _data, bool candirecthit = true) {
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
        throwable.Initialize(classname, user, firedfrom, speed, range, torque, _data, candirecthit);
        projectiles.Add(throwable);
        return throwable;
    }
}
