using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 이펙트 생성 클래스
 * 여기서는 오브젝트에 부착된 이펙트, 오브젝트와 독립된 이펙트, 오브젝트와 독립되어 특정 방향으로 움직이는 이펙트 생성 가능 */
public static class EffectManager {
	private static List<Effect> effects = new List<Effect>();

    private static EntityManager em;
    public static void SetEntityManager(EntityManager _em) {
        em = _em;
    }

    public static List<Effect> GetEffects() {
		return effects;
	}

	public static void RemoveEffect(Effect effect) {
		effects.Remove(effect);
        em.AddEffectToPool(effect.gameObject);
	}

    public static void CreateEffect(string classname, Vector3 pos, float angle, Transform parent = null, bool flip = false) {
		if(!LevelManager.instance.isMapActive) return;

        GameObject effect_obj = em.GetEffectFromPool();
        effect_obj.transform.position = pos;

        Vector3 ang = effect_obj.transform.eulerAngles;
        ang.z = angle;
        effect_obj.transform.eulerAngles = ang;

        effect_obj.GetComponent<SpriteRenderer>().flipX = flip;
        effect_obj.GetComponent<Effect>().Initialize(classname);
        effect_obj.GetComponent<Rigidbody2D>().velocity = new Vector2();

        if (parent != null) {
            effect_obj.transform.SetParent(parent);
        }
		
    }

    public static void CreateMovingEffect(string classname, Vector3 pos, Vector2 vel, float angle, int dir = 1, Transform parent = null) {
		if(!LevelManager.instance.isMapActive) return;

        GameObject effect_obj = em.GetEffectFromPool();

        effect_obj.transform.position = pos;

        Vector3 lscale = effect_obj.transform.localScale;
        lscale.x = lscale.x * dir;
        effect_obj.transform.localScale = lscale;

		Vector3 ang = effect_obj.transform.eulerAngles;
		ang.z = angle;
		effect_obj.transform.eulerAngles = ang;

		effect_obj.GetComponent<Effect>().Initialize(classname);
        effect_obj.GetComponent<Rigidbody2D>().velocity = vel;

        if (parent != null) {
            effect_obj.transform.SetParent(parent);
        }
    }
}
