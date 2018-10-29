using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 투사체 생성 클래스
 * 여기서는 총알, 포환, 투척물을 생성할 수 있음. */
public static class BulletManager {
    private static List<Bullet> bullets = new List<Bullet>();
    private static List<Projectile> projectiles = new List<Projectile>();
    private static List<Laser> lasers = new List<Laser>();

    private static EntityManager em;
    public static void SetEntityManager(EntityManager _em) {
        em = _em;
    }

    public static List<Bullet> GetBullets() {
        return bullets;
    }

    public static void OnBulletDestroy(Bullet b) {
        bullets.Remove(b);
        em.AddBulletToPool(b.gameObject);
        UnityEngine.Object.Destroy(b);
    }

    public static List<Projectile> GetProjectiles() {
        return projectiles;
    }

    public static void OnProjectileDestroy(Projectile p) {
        projectiles.Remove(p);
        em.AddProjectileToPool(p.gameObject);
        UnityEngine.Object.Destroy(p);
    }

    public static List<Laser> GetLasers() {
        return lasers;
    }

    public static void OnLaserDestroy(Laser l) {
        lasers.Remove(l);
        em.AddLaserToPool(l.gameObject);
        UnityEngine.Object.Destroy(l);
    }

    public static Bullet CreateBullet(string classname, Vector3 pos, Character user, Weapon firedfrom, float angle, Dictionary<WeaponStats, float> _data, bool ignoreGround = false) {
		if(!LevelManager.instance.isMapActive)
			return null;

        GameObject bullet_obj = em.GetBulletFromPool();
        bullet_obj.transform.position = pos;

		JDictionary scriptData = GameDataManager.instance.RootData[classname]["ScriptClass"];
		string script = (scriptData ? scriptData.Value<string>() : "Bullet");
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

    public static Projectile CreateProjectile(string classname, Vector3 pos, Character user, Weapon firedfrom, float speed, float range, float angle, Dictionary<WeaponStats, float> _data, bool candirecthit) {
		if(!LevelManager.instance.isMapActive)
			return null;

        GameObject projectile_obj = em.GetProjectileFromPool();
        projectile_obj.transform.position = pos;

        string script = GameDataManager.instance.RootData[classname]["ScriptClass"].Value<string>();
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

    public static Throwable CreateThrowable(string classname, Vector3 pos, Character user, Weapon firedfrom, float speed, float range, float angle, float torque, Dictionary<WeaponStats, float> _data, bool candirecthit = true) {
		if(!LevelManager.instance.isMapActive)
			return null;

        GameObject throwable_obj = em.GetProjectileFromPool();
        throwable_obj.transform.position = pos;

        string script = GameDataManager.instance.RootData[classname]["ScriptClass"].Value<string>();
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

    public static Laser CreateLaser(string classname, Vector3 pos, Character user, Weapon firedfrom, float angle, Dictionary<WeaponStats, float> _data) {
        if (!LevelManager.instance.isMapActive)
			return null;

        GameObject laser_obj = em.GetLaserFromPool();
        laser_obj.transform.position = pos;

        Laser l = laser_obj.AddComponent<Laser>();
		float laserWidth = GameDataManager.instance.RootData[classname]["LaserWidth"].Value<float>();
		l.Initialize(classname, user, firedfrom, pos, angle, _data[WeaponStats.Range], laserWidth, _data);

        lasers.Add(l);

		return l;
    }
}
