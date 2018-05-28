﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 이펙트 생성 클래스
 * 여기서는 오브젝트에 부착된 이펙트, 오브젝트와 독립된 이펙트, 오브젝트와 독립되어 특정 방향으로 움직이는 이펙트 생성 가능 */
public class EffectManager : MonoBehaviour {
    public static EffectManager instance;
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void CreateEffect(string classname, Vector3 pos, int dir, Transform parent = null) {
        GameObject effect_obj = (GameObject)Instantiate(Resources.Load("Prefab/Effect"), pos, new Quaternion());
        if(dir != 0) {
            Vector3 lscale = effect_obj.transform.localScale;
            lscale.x = lscale.x * dir;
            effect_obj.transform.localScale = lscale;
        }
        effect_obj.GetComponent<Effect>().Initialize(classname);
        if(parent != null) {
            effect_obj.transform.SetParent(parent);
        }
    }

    public void CreateEffect(string classname, Vector3 pos, float angle, Transform parent = null) {
        GameObject effect_obj = (GameObject)Instantiate(Resources.Load("Prefab/Effect"), pos, new Quaternion());
        Vector3 ang = effect_obj.transform.eulerAngles;
        ang.z = angle;
        effect_obj.transform.eulerAngles = ang;
        effect_obj.GetComponent<Effect>().Initialize(classname);
        if (parent != null) {
            effect_obj.transform.SetParent(parent);
        }
    }

    public void CreateMovingEffect(string classname, Vector3 pos, Vector2 vel, int dir, Transform parent = null) {
        GameObject effect_obj = (GameObject)Instantiate(Resources.Load("Prefab/Effect_Moving"), pos, new Quaternion());
        Vector3 lscale = effect_obj.transform.localScale;
        lscale.x = lscale.x * dir;
        effect_obj.transform.localScale = lscale;
        effect_obj.GetComponent<Effect>().Initialize(classname);
        effect_obj.GetComponent<Rigidbody2D>().velocity = vel;
        if (parent != null) {
            effect_obj.transform.SetParent(parent);
        }
    }

    public void CreateMovingEffect(string classname, Vector3 pos, Vector2 vel, float angle, Transform parent = null) {
        GameObject effect_obj = (GameObject)Instantiate(Resources.Load("Prefab/Effect_Moving"), pos, new Quaternion());
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
