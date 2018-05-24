using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour {

    protected float maxSpeed = 0.0f;
    protected float accel = 20f;
    protected float realSpeed = 0.0f;
    protected float dir = 0;
    protected Character character;

    public virtual void Initialize(Character c) {
        character = c;
        maxSpeed = character.GetCurrentStat(CharacterStats.MoveSpeed);
        realSpeed = 0.0f;
    }

    public void Move(float a) {
        dir = a;
        maxSpeed = character.GetCurrentStat(CharacterStats.MoveSpeed);
        realSpeed = Mathf.Clamp(realSpeed + maxSpeed / 3f, 0, maxSpeed);
    }

    public void AddForce(Vector2 force) {
        GetComponent<Rigidbody2D>().velocity += force;
    }

    void FixedUpdate () {
        if (character == null) return;
        if (character.GetUncontrollableTimeLeft() > 0) {
            realSpeed = 0.0f;
            return;
        }
        GetComponent<Rigidbody2D>().velocity = new Vector2(dir * realSpeed, GetComponent<Rigidbody2D>().velocity.y);
        dir = 0;
    }
}
