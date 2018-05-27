using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    protected Character attacker;
    protected Weapon weapon;
    protected float speed;
    protected float range;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Vector3 collisionPos;
    protected Vector3 collisionNorm;
    protected Vector3 startPos;
    protected Vector3 velVector;
    protected string className;
    protected bool collided = false;
    protected bool init = false;
    protected LayerMask layer;
    protected Dictionary<WeaponStats, float> data;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        collided = false;
        layer = (1 << LayerMask.NameToLayer("Characters"));// | (1 << LayerMask.NameToLayer("Characters"));
    }
    public void Initialize(string classname, Character user, Weapon firedfrom, Dictionary<WeaponStats, float> _data, bool candirecthit) {
        className = classname;
        attacker = user;
        weapon = firedfrom;
        speed = _data[WeaponStats.BulletSpeed];
        range = _data[WeaponStats.Range];
        data = _data;
        startPos = transform.position;
        if(GameDataManager.instance.GetData("Data", className, "Sprites", "smoke") != null)
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "smoke"), transform.position, transform.eulerAngles.z, transform);
        if (GameDataManager.instance.GetAnimatorController(classname) != null)
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);
        else {
            transform.localScale = new Vector3(0, 0, 0);
            anim = null;
        }
        rb.velocity = transform.right * speed;
        velVector = transform.right;
        init = true;
        AdditionalData();
        if (candirecthit)
            StartCoroutine(DirectHit());
    }

    /*private void Update() {
        Collider2D[] colliding = Physics2D.OverlapBoxAll(transform.position, new Vector2(128, 128), 0, layer);
        foreach (Collider2D collider in colliding) {
            if (collider.GetComponent<Character>() != null && collider.GetComponent<Character>().GetTeam() == attacker.GetTeam()) {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.GetComponents<Collider2D>()[0]);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.GetComponents<Collider2D>()[1]);
            }
        }
    }*/

    protected IEnumerator DirectHit() {
        yield return new WaitWhile(() => Physics2D.OverlapBox(transform.position, new Vector2(4, 4), 0, layer));
        gameObject.layer = LayerMask.NameToLayer("Bullet");
    }

    protected virtual void AdditionalData() {

    }

    protected virtual void FixedUpdate() {
        if (!init) return;
        rb.velocity += new Vector2(0, -981f * Time.fixedDeltaTime);
        velVector = rb.velocity.normalized;
        float ang = Helper.Vector2ToAng(rb.velocity);
        if (ang >= 90 && ang <= 270) {
            GetComponent<SpriteRenderer>().flipY = true;
        }
        else {
            GetComponent<SpriteRenderer>().flipY = false;
        }
        Vector3 proang = transform.eulerAngles;
        proang.z = ang;
        transform.eulerAngles = proang;
        Collider2D[] colliding = Physics2D.OverlapBoxAll(transform.position, new Vector2(128, 128), 0, layer);
        foreach (Collider2D collider in colliding) {
            if (collider.GetComponent<Character>() != null && collider.GetComponent<Character>().GetTeam() == attacker.GetTeam()) {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.GetComponents<Collider2D>()[0]);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collider.GetComponents<Collider2D>()[1]);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        collisionPos = collision.contacts[0].point;
        collisionNorm = collision.contacts[0].normal;
        HandleCollision(collision);
    }

    private void HandleCollision(Collision2D collision) {
        if (collision.gameObject == null) return;
        if (collision.gameObject.tag.Equals("Character")) {
            if(collision.gameObject.GetComponent<Character>().GetTeam() != attacker.GetTeam()) {
                collided = true;
                DestroyObject(gameObject);
                init = false;
                return;
            }
            else {
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collision.gameObject.GetComponents<Collider2D>()[0]);
                Physics2D.IgnoreCollision(GetComponents<Collider2D>()[0], collision.gameObject.GetComponents<Collider2D>()[1]);
                return;
            }
        }
        else if (collision.gameObject.tag.Equals("Ground") || collision.gameObject.tag.Equals("Ceiling")) {
            collided = true;
            DestroyObject(gameObject);
            init = false;
            return;
        }
    }

    protected virtual void OnDestroy() {
        if (!collided) return;
        if(GameDataManager.instance.GetData("Data", className, "Sprites", "ShakeCam") == null || (GameDataManager.instance.GetData("Data", className, "Sprites", "ShakeCam") != null && (int)GameDataManager.instance.GetData("Data", className, "Sprites", "ShakeCam") != 0))
            CamController.instance.ShakeCam(data[WeaponStats.Damage] / 20f, Mathf.Clamp(data[WeaponStats.Damage] / 50, 0, 2));
        List<Character> closeEnemies = CharacterManager.instance.GetEnemies(attacker.GetTeam()).FindAll(c => Vector3.Distance(c.transform.position, transform.position) <= range);
        foreach (Character c in closeEnemies) {
            DamageData dmgdata = Helper.DamageCalc(attacker, data, c, true);
            c.DoDamage(attacker, dmgdata.damage, data[WeaponStats.Stagger]);
        }
        if (anim != null && GameDataManager.instance.GetData("Data", className, "Sprites", "hit") != null) {
            Vector3 temp = collisionNorm;
            temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
            float ang = Helper.Vector2ToAng(temp);
            EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData("Data", className, "Sprites", "hit"), collisionPos, ang);
        }
    }
}
