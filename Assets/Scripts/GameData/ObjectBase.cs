using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ObjectBase : MonoBehaviour {
    public bool isStatic = true;
    protected LayerMask groundLayer;
    protected Animator anim;
    protected BoxCollider2D box;
    protected BoxCollider2D nofriction;
    protected string className;
    protected bool onGround = true;
    protected bool facingRight = true;
    protected float height = 1;
    protected int pixelsPerUnit = 1;
    public virtual void Initialize(string classname) {
        className = classname;
        anim = GetComponent<Animator>();
        if (GetComponents<BoxCollider2D>().Count() > 0) {
            box = GetComponents<BoxCollider2D>()[0];
            nofriction = GetComponents<BoxCollider2D>()[1];
            isStatic = false;
        }
        if (!isStatic) {
            if (GameDataManager.instance.GetData("Data", classname, "Collider") != null) {
                box.offset = new Vector2((float)GameDataManager.instance.GetData("Data", classname, "Collider", "Offset_X")
                                            , (float)GameDataManager.instance.GetData("Data", classname, "Collider", "Offset_Y"));
                box.size = new Vector2((float)GameDataManager.instance.GetData("Data", classname, "Collider", "Size_X")
                                                , (float)GameDataManager.instance.GetData("Data", classname, "Collider", "Size_Y"));
                nofriction.offset = new Vector2((float)GameDataManager.instance.GetData("Data", classname, "Collider", "Offset_X")
                                                , (float)GameDataManager.instance.GetData("Data", classname, "Collider", "Offset_Y") + 0.5f);
                nofriction.size = new Vector2((float)GameDataManager.instance.GetData("Data", classname, "Collider", "Size_X")
                                                , (float)GameDataManager.instance.GetData("Data", classname, "Collider", "Size_Y") - 1);
            }
        }
        if (GameDataManager.instance.GetData("Data", classname, "Scale") != null) {
            transform.localScale = new Vector3((float)GameDataManager.instance.GetData("Data", classname, "Scale", "X")
                                            , (float)GameDataManager.instance.GetData("Data", classname, "Scale", "Y"), 1);
        }
        if (GameDataManager.instance.GetAnimatorController(classname) != null)
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else {
            Color c = GetComponent<SpriteRenderer>().color;
            c.a = 0;
            GetComponent<SpriteRenderer>().color = c;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
    
    public bool IsOnGround() {
        return onGround;
    }

    public bool IsFacingRight() {
        return facingRight;
    }

    public int GetFacingDirection() {
        return (int)((Convert.ToSingle(facingRight) - 0.5f) * 2f);
    }

    public string GetClass() {
        return className;
    }

    public Animator GetAnimator() {
        return anim;
    }

    public BoxCollider2D GetCollider() {
        return box;
    }

    public void FlipFace(bool right) {
        facingRight = right;
        Vector3 lscale = transform.localScale;
        if (right)
            lscale.x = (float)GameDataManager.instance.GetData("Data", className, "Scale", "X") * 1;
        else
            lscale.x = (float)GameDataManager.instance.GetData("Data", className, "Scale", "X") * -1;
        transform.localScale = lscale;
    }

    public void EnableCollision(bool enable) {
        if (GetComponents<BoxCollider2D>().Count() > 0) {
            foreach(BoxCollider2D col in GetComponents<BoxCollider2D>()) {
                col.enabled = enable;
            }
        }
    }

    public void EnableRigidbody(bool enable) {
        if(GetComponent<Rigidbody2D>() != null) {
            if (!enable) {
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                GetComponent<Rigidbody2D>().Sleep();
            }
            else {
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                GetComponent<Rigidbody2D>().WakeUp();
            }
        }
    }

    public void ModifyHeight(float mult) {
        if (isStatic || mult == height)
            return;
        if (GameDataManager.instance.GetData("Data", className, "Collider") != null) {
            float originalheight = (float)GameDataManager.instance.GetData("Data", className, "Collider", "Size_Y");
            float difference = originalheight * mult - originalheight * height;
            //if(difference < 0)
            //    transform.position = transform.position + new Vector3(0, difference / 2f, 0);
            box.offset = new Vector2((float)GameDataManager.instance.GetData("Data", className, "Collider", "Offset_X")
                                        , box.offset.y + difference / 2f);
            box.size = new Vector2((float)GameDataManager.instance.GetData("Data", className, "Collider", "Size_X")
                                            , originalheight * mult);
            nofriction.offset = new Vector2((float)GameDataManager.instance.GetData("Data", className, "Collider", "Offset_X")
                                            , box.offset.y + 0.5f);
            nofriction.size = new Vector2((float)GameDataManager.instance.GetData("Data", className, "Collider", "Size_X") + 3f
                                            , originalheight * mult - 1);
            height = mult;
        }
    }

    protected virtual void FixedUpdate() {
        if (isStatic)
            return;
        onGround = Physics2D.OverlapBox((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f - 6f), new Vector2(box.size.x + 0.5f, 16f), 0, groundLayer)
            && ((GetComponent<Rigidbody2D>().velocity.y <= 5f) || Physics2D.OverlapBox((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f - 16f), new Vector2(box.size.x + 0.5f, 12f), 0, groundLayer) == null);
        if (anim != null)
            anim.SetBool("onGround", onGround);
    }

    protected virtual void Awake() {
        groundLayer = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Ceiling"));
    }
}