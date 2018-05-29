using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : MonoBehaviour {

    public int id;
    public float duration = 0;
    public float burnduration = 2f;
    public float damage = 0;
    public float tickdelay = 0;
    public Teams team = Teams.None;
    private BoxCollider2D box;

    private void Awake() {
        box = GetComponent<BoxCollider2D>();
        id = GetInstanceID();
    }

    public void SetData(int _id, float _duration, float _damage, float _tickdelay, float _burnduration, Teams _team = Teams.None) {
        id = _id;
        duration = _duration;
        damage = _damage;
        tickdelay = _tickdelay;
        burnduration = _burnduration;
        team = _team;
    }

    private void Die() {
        GetComponent<Animator>().SetTrigger("End");
        StartCoroutine(WaitDeath());
    }

    IEnumerator WaitDeath() {
        yield return new WaitWhile(() => GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < 0.95);
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(gameObject);
    }

    void Update() {
        duration -= Time.deltaTime;
        if (duration <= 0)
            Die();
        Collider2D[] colliding = Physics2D.OverlapBoxAll(transform.TransformPoint(box.offset), box.size, 0, Helper.characterLayer);
        foreach(Collider2D collider in colliding) {
            if (collider.GetComponentInChildren<Character>() != null && collider.GetComponentInChildren<Character>().GetTeam() != team)
                collider.GetComponentInChildren<Character>().AddDOT(id, burnduration, damage, tickdelay, null);
        }
    }
}
