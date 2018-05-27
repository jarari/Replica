using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour {

    protected float maxSpeed = 0.0f;
    protected float accel = 20f;
    protected float realSpeed = 0.0f;
    protected float dir = 0;
    protected float lastdir = 0;
    protected bool movedThisFrame = false;
    protected Character character;

    public virtual void Initialize(Character c) {
        character = c;
        maxSpeed = character.GetCurrentStat(CharacterStats.MoveSpeed);
        realSpeed = 0.0f;
    }

    public void Move(float a) {
        movedThisFrame = true;
        lastdir = dir;
        dir = a;
        maxSpeed = character.GetCurrentStat(CharacterStats.MoveSpeed);
        realSpeed = Mathf.Clamp(realSpeed + maxSpeed / 5f, 0, maxSpeed);
        if(Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) < Mathf.Abs(dir * realSpeed)) {
            float velX = Mathf.Clamp(dir * realSpeed, -maxSpeed, maxSpeed);
            GetComponent<Rigidbody2D>().velocity = new Vector2(velX, GetComponent<Rigidbody2D>().velocity.y);
        }
        else if(dir * GetComponent<Rigidbody2D>().velocity.x < 0){
            float velX = GetComponent<Rigidbody2D>().velocity.x + dir * realSpeed;
            GetComponent<Rigidbody2D>().velocity = new Vector2(velX, GetComponent<Rigidbody2D>().velocity.y);
        }
    }

    public void ForceMove(float a) {
        maxSpeed = character.GetCurrentStat(CharacterStats.MoveSpeed);
        realSpeed = maxSpeed;
        GetComponent<Rigidbody2D>().velocity = new Vector2(a * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
    }

    public void AddForce(Vector2 force) {
        GetComponent<Rigidbody2D>().velocity += force;
    }

    void LateUpdate () {
        if (character == null) return;
        if (character.GetUncontrollableTimeLeft() > 0 || !movedThisFrame || lastdir * dir < 0) {
            realSpeed = 0.0f;
            return;
        }
        //GetComponent<Rigidbody2D>().velocity = new Vector2(dir * realSpeed, GetComponent<Rigidbody2D>().velocity.y);
        dir = 0;
    }
}
