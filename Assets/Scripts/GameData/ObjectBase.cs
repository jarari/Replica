using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ObjectBase : MonoBehaviour {
    public bool isStatic = true;
    protected Animator anim;
    protected BoxCollider2D box;
    protected BoxCollider2D nofriction;
    protected Rigidbody2D rb;
    protected Inventory inventory;
    protected string className;
    protected bool onGround = true;
    protected bool facingRight = true;
    protected float height = 1;

	protected JDictionary objectData;

    public virtual void Initialize(string classname) {
        className = classname;
		this.objectData = GameDataManager.instance.RootData[className];

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (GetComponents<BoxCollider2D>().Count() > 0) {
            box = GetComponents<BoxCollider2D>()[0];
            if(GetComponents<BoxCollider2D>().Count() > 1)
                nofriction = GetComponents<BoxCollider2D>()[1];
        }

		if(rb)
            isStatic = false;

        if (box) {
			JDictionary colliderData = objectData["Collider"];
			if (colliderData) {
                box.offset = new Vector2(
					colliderData["Offset_X"].Value<float>(),
					colliderData["Offset_Y"].Value<float>()
					);

                box.size = new Vector2(
					colliderData["Size_X"].Value<float>(),
					colliderData["Size_Y"].Value<float>()
					);

                if(nofriction) {
                    nofriction.offset = new Vector2(
						colliderData["Offset_X"].Value<float>(),
						colliderData["Offset_Y"].Value<float>() + 0.5f
						);
                    nofriction.size = new Vector2(
						colliderData["Size_X"].Value<float>(),
						colliderData["Size_Y"].Value<float>() - 1
						);
                }
            }
        }

        if (objectData["Scale"]) {
            transform.localScale = new Vector3(
				objectData["Scale"]["X"].Value<float>(),
				objectData["Scale"]["Y"].Value<float>(), 
				1.0f
				);
        }

		RuntimeAnimatorController animCtrl = GameDataManager.instance.GetAnimatorController(className);
		if (animCtrl)
			anim.runtimeAnimatorController = animCtrl;
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

    public Rigidbody2D GetRigidbody() {
        return rb;
    }

    public void FlipFace(bool right) {
		facingRight = right;

		float sourceScale = objectData["Scale"]["X"].Value<float>();
		Vector3 lscale = transform.localScale;
		lscale.x = sourceScale * (right ? 1.0f : -1.0f);
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

		JDictionary colliderData = objectData["Collider"];
		if (colliderData) {
            float originalheight = colliderData["Size_Y"].Value<float>();
            float difference = originalheight * mult - originalheight * height;

            //if(difference < 0)
            //    transform.position = transform.position + new Vector3(0, difference / 2f, 0);

            box.offset = new Vector2(
				colliderData["Offset_X"].Value<float>(), 
				box.offset.y + difference / 2f
				);

            box.size = new Vector2(
				colliderData["Size_X"].Value<float>(), 
				originalheight * mult
				);

            if(nofriction) {
                nofriction.offset = new Vector2(
					colliderData["Offset_X"].Value<float>(), 
					box.offset.y + 0.5f
					);

                nofriction.size = new Vector2(
					colliderData["Size_X"].Value<float>(), 
					originalheight * mult - 1
					);
            }
            height = mult;
        }
    }

    public void SetInventory(Inventory i) {
        inventory = i;
    }

    public Inventory GetInventory() {
        return inventory;
    }

    protected virtual void FixedUpdate() {
        if (isStatic)
            return;
        onGround = Physics2D.OverlapBox((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f - 6f), new Vector2(box.size.x + 0.5f, 16f), 0, Helper.mapLayer) != null
            && ((GetComponent<Rigidbody2D>().velocity.y <= 5f) || Physics2D.OverlapBox((Vector2)transform.position + box.offset - new Vector2(0, box.size.y / 2f - 16f), new Vector2(box.size.x + 0.5f, 12f), 0, Helper.mapLayer) == null);
        if (anim != null)
            anim.SetBool("onGround", onGround);
    }

    protected virtual void LateUpdate() {
        Vector2 pos = transform.position;
        pos.x = Mathf.Round(pos.x * Helper.PixelsPerUnit) / Helper.PixelsPerUnit;
        pos.y = Mathf.Round(pos.y * Helper.PixelsPerUnit) / Helper.PixelsPerUnit;
        transform.position = pos;
    }
}