using UnityEngine;

public abstract class Controller : MonoBehaviour {
    protected Character character;
    protected BoxCollider2D box;
    public virtual void Initialize(Character c) {
        character = c;
        box = c.GetCollider();
    }

    protected virtual void Update() {

    }

    public abstract void ResetAttackTimer();
}