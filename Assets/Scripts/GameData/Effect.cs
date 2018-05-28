using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour {
    private Animator anim;
    private int Loop = 0;
    private int Flip = 0;
    void Awake() {
        anim = GetComponent<Animator>();
    }

    public void Initialize(string classname) {
        if (GameDataManager.instance.GetAnimatorController(classname) != null)
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else
            transform.localScale = new Vector3(0, 0, 0);
        Loop = (int)GameDataManager.instance.GetData("Data", classname, "Loop");
        if (Loop == 0)
            StartCoroutine(DestroyEffect());
        if (GameDataManager.instance.GetData("Data", classname, "Flip") != null)
            Flip = (int)GameDataManager.instance.GetData("Data", classname, "Flip");
        Vector3 temp = transform.localScale;
        if (GameDataManager.instance.GetData("Data", classname, "Scale") != null) {
            temp.x *= (float)GameDataManager.instance.GetData("Data", classname, "Scale", "X");
            temp.y *= (float)GameDataManager.instance.GetData("Data", classname, "Scale", "Y");
        }
        transform.localScale = temp;
        temp = transform.eulerAngles;
        if(GameDataManager.instance.GetData("Data", classname, "Rotation") != null) {
            temp.z += (float)GameDataManager.instance.GetData("Data", classname, "Rotation");
        }
        transform.eulerAngles = temp;
        temp = new Vector3(0, 0, 0);
        if (GameDataManager.instance.GetData("Data", classname, "Position") != null) {
            temp.x = (float)GameDataManager.instance.GetData("Data", classname, "Position", "X");
            temp.y = (float)GameDataManager.instance.GetData("Data", classname, "Position", "Y");
        }
        if(GameDataManager.instance.GetData("Data", classname, "ShouldDisplayBeneathGround") != null
            && (int)GameDataManager.instance.GetData("Data", classname, "ShouldDisplayBeneathGround") == 1) {
            GetComponent<SpriteRenderer>().sortingLayerName = "DustEffect";
        }
        if (GameDataManager.instance.GetData("Data", classname, "SortingOrder") != null) {
            GetComponent<SpriteRenderer>().sortingOrder = Convert.ToInt32(GameDataManager.instance.GetData("Data", classname, "SortingOrder"));
        }
        transform.position = transform.TransformPoint(temp);
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
        DestroyObject(gameObject);
    }
}
