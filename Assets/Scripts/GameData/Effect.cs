using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour {
    private Animator anim;
    private int Loop = 0;
    private int Flip = 0;

	private JDictionary effectData;

    void Awake() {
        anim = GetComponent<Animator>();
    }

    public void Initialize(string classname) {
        if (GameDataManager.instance.GetAnimatorController(classname))
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else
            transform.localScale = new Vector3(0, 0, 0);

		effectData = GameDataManager.instance.RootData[classname];

		Loop = effectData["Loop"].Value<int>();
        if (Loop == 0)
            StartCoroutine(DestroyEffect());

        if (effectData["Flip"])
            Flip = effectData["Flip"].Value<int>();

        Vector3 temp = transform.localScale;
        if (effectData["Scale"]) {
            temp.x *= effectData["Scale"]["X"].Value<float>();
            temp.y *= effectData["Scale"]["Y"].Value<float>();
        }
        transform.localScale = temp;

        temp = transform.eulerAngles;
        if(effectData["Rotation"]) {
            temp.z += effectData["Rotation"].Value<float>();
        }
        transform.eulerAngles = temp;

        temp = new Vector3(0, 0, 0);
        if (effectData["Position"]) {
            temp.x = effectData["Position"]["X"].Value<float>();
            temp.y = effectData["Position"]["Y"].Value<float>();
		}
		transform.position = transform.TransformPoint(temp);

		if(effectData["ShouldDisplayBeneathGround"] && effectData["ShouldDisplayBeneathGround"].Value<int>() == 1) {
            GetComponent<SpriteRenderer>().sortingLayerName = "DustEffect";
        }

        if (effectData["SortingOrder"]) {
            GetComponent<SpriteRenderer>().sortingOrder = effectData["SortingOrder"].Value<int>();
        }

        if (effectData["Material"]) {
            GetComponent<SpriteRenderer>().material = Helper.GetMaterial("Sprites/shader/", effectData["Material"].Value<string>());
        }
    }

    private void Update() {
        if (Flip == 1) {
            float ang = transform.eulerAngles.z;
            if (transform.parent != null)
                ang = transform.parent.eulerAngles.z;
            if (ang >= 90 && ang <= 270) {
                GetComponent<SpriteRenderer>().flipY = true;
            }
            else {
                GetComponent<SpriteRenderer>().flipY = false;
            }
        }
    }

    IEnumerator DestroyEffect() {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

	// 이펙트 오브젝트 제거시에 리스트도 즉시 제거
	private void OnDestroy() {
		EffectManager.instance.RemoveEffect(this);
	}
}
