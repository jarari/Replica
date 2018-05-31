using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 모든 움직일 수 있는 물체의 기본이 되는 컨트롤러..로 쓰려 했지만
 * 캐릭터의 기반이 됨.*/
public class MoveObject : MonoBehaviour {

    protected float maxSpeed = 0.0f;
    protected float accel = 20f;
    protected float realSpeed = 0.0f;
    protected float movedir = 0;
    protected float lastdir = 0;
    protected bool movedThisFrame = false;
    protected Character character;

    public virtual void Initialize(Character c) {
        character = c;
        maxSpeed = character.GetCurrentStat(CharacterStats.MoveSpeed);
        realSpeed = 0.0f;
    }

    /* 이동속도에 비례해서 움직이기.
     * a 값으로 속도를 조절할 수는 있지만 최대 속도를 넘어설 수는 없음 */
    public void Move(float a) {
        movedThisFrame = true;
        lastdir = movedir;
        movedir = a;
        maxSpeed = character.GetCurrentStat(CharacterStats.MoveSpeed);
        realSpeed = Mathf.Clamp(realSpeed + maxSpeed / 15f, 0, maxSpeed);
        if(Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) < Mathf.Abs(movedir * realSpeed)) {
            float velX = Mathf.Clamp(movedir * realSpeed, -maxSpeed, maxSpeed);
            GetComponent<Rigidbody2D>().velocity = new Vector2(velX, GetComponent<Rigidbody2D>().velocity.y);
        }
        else if(movedir * GetComponent<Rigidbody2D>().velocity.x < 0){
            float velX = GetComponent<Rigidbody2D>().velocity.x + movedir * realSpeed;
            GetComponent<Rigidbody2D>().velocity = new Vector2(velX, GetComponent<Rigidbody2D>().velocity.y);
        }
    }

    /* 최고 속도로 즉시 움직일 떄 사용 */
    public void ForceMove(float a) {
        maxSpeed = character.GetCurrentStat(CharacterStats.MoveSpeed);
        realSpeed = maxSpeed;
        GetComponent<Rigidbody2D>().velocity = new Vector2(a * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
    }

    /* 밀어내기 */
    public void AddForce(Vector2 force) {
        GetComponent<Rigidbody2D>().velocity += force;
    }

    /* y 가속도 강제 오버라이드 */
    public void SetVelY(float v) {
        GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, v);
    }

    void LateUpdate () {
        if (character == null) return;
        if (character.GetUncontrollableTimeLeft() > 0 || !movedThisFrame || lastdir * movedir < 0) {
            realSpeed = 0.0f;
            return;
        }
        movedThisFrame = false;
        //GetComponent<Rigidbody2D>().velocity = new Vector2(movedir * realSpeed, GetComponent<Rigidbody2D>().velocity.y);
        movedir = 0;
    }
}
