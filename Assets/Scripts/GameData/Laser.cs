using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {
    public MeshFilter mf;
    public MeshRenderer mr;
    public Animator anim;
    public void Initialize(string classname, Character attacker, Weapon weapon, Vector3 startPos, float angle, float distance, float width, Dictionary<WeaponStats, float> data) {

        mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        mr = GetComponent<MeshRenderer>();
        mr.sortingLayerName = "Ground";

        anim = GetComponent<Animator>();
        if (GameDataManager.instance.GetAnimatorController(classname) != null)
            anim.runtimeAnimatorController = GameDataManager.instance.GetAnimatorController(classname);

        transform.position = startPos;

        List<Vector3> vertices = new List<Vector3>();
        float radAng = Mathf.Deg2Rad * angle;
        Vector3 dir = new Vector3(Mathf.Cos(radAng), Mathf.Sin(radAng), 0);
        RaycastHit2D rayhit = Physics2D.Raycast(startPos, dir, distance, Helper.mapLayer | Helper.characterLayer);
        Vector3 endPos = startPos + dir * distance;
        if (rayhit.collider != null) {
            endPos = (Vector3)rayhit.point + dir * 5f;
            if (GameDataManager.instance.GetData(classname, "Sprites", "hit") != null) {
                Vector3 temp = rayhit.normal;
                temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
                float ang = Helper.Vector2ToAng(temp);
                EffectManager.instance.CreateEffect((string)GameDataManager.instance.GetData(classname, "Sprites", "hit"), endPos, ang);
				ParticleManager.instance.CreateParticle("particle_laser", endPos, ang);
				ParticleManager.instance.CreateParticle("particle_greenlaserlight", endPos, ang);
            }
        }
        Vector3 dtraj = endPos - startPos;
        Vector3 up = Vector3.Cross(dir, Vector3.forward);
        vertices.Add(up * width / 2f);
        vertices.Add(-up * width / 2f);
        vertices.Add(dtraj + up * width / 2f);
        vertices.Add(dtraj - up * width / 2f);
        mesh.Clear();
        mesh.vertices = vertices.ToArray();

        mesh.triangles = new int[] { 2, 1, 0, 1, 2, 3 };

        mesh.uv = new Vector2[] { new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 0) };

        StartCoroutine(DestroyEffect());
        
        Collider2D[] colliders = Physics2D.OverlapBoxAll((startPos + endPos) / 2f, new Vector2(Mathf.Abs(dtraj.x), width), angle, Helper.characterLayer);
        foreach(Collider2D col in colliders) {
            Character c = col.GetComponent<Character>();
            if(c != null) {
                if(c.GetTeam() != attacker.GetTeam()) {
                    DamageData dmgData = Helper.DamageCalc(attacker, data, c, true);
                    c.DoDamage(attacker, dmgData.damage, dmgData.stagger);
                }
            }
        }
    }

    IEnumerator DestroyEffect() {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        DestroyObject(gameObject);
    }
}