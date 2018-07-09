using System;
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
		if(!LevelManager.instance.isMapActive) return null;
		GameObject bullet_obj = (GameObject)Instantiate(Resources.Load("Prefab/Bullet"), pos, new Quaternion());
        string script = (string)GameDataManager.instance.GetData(classname, "ScriptClass");
        if (script == null || script.Length == 0)
            script = "Bullet";
        Bullet bullet = (Bullet)bullet_obj.AddComponent(Type.GetType(script));
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
		if(!LevelManager.instance.isMapActive) return null;
		GameObject projectile_obj = (GameObject)Instantiate(Resources.Load("Prefab/Projectile"), pos, new Quaternion());
        string script = (string)GameDataManager.instance.GetData(classname, "ScriptClass");
        if (script == null || script.Length == 0)
            script = "Projectile";
        Projectile projectile = (Projectile)projectile_obj.AddComponent(Type.GetType(script));
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
		if(!LevelManager.instance.isMapActive) return null;
		GameObject throwable_obj = (GameObject)Instantiate(Resources.Load("Prefab/Projectile"), pos, new Quaternion());
        string script = (string)GameDataManager.instance.GetData(classname, "ScriptClass");
        if (script == null || script.Length == 0)
            script = "Throwable";
        Throwable throwable = (Throwable)throwable_obj.AddComponent(Type.GetType(script));
        Physics2D.IgnoreCollision(throwable_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[0]);
        Physics2D.IgnoreCollision(throwable_obj.GetComponents<Collider2D>()[0], user.GetComponentsInParent<Collider2D>()[1]);
        Vector3 ang = throwable_obj.transform.eulerAngles;
        ang.z = angle;
        throwable_obj.transform.eulerAngles = ang;
        throwable.Initialize(classname, user, firedfrom, speed, range, torque, _data, candirecthit);
        projectiles.Add(throwable);
        return throwable;
    }

    public Laser CreateLaser(string classname, Vector3 pos, Character user, Weapon firedfrom, float angle, Dictionary<WeaponStats, float> _data) {
        if (!LevelManager.instance.isMapActive) return null;
        GameObject laser_obj = (GameObject)Instantiate(Resources.Load("Prefab/Laser"), pos, new Quaternion());
        Laser l = laser_obj.GetComponent<Laser>();
        l.Initialize(classname, user, firedfrom, pos, angle, _data[WeaponStats.Range], Convert.ToSingle(GameDataManager.instance.GetData(classname, "LaserWidth")), _data);
        return l;
    }
}
