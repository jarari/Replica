using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {
    public MeshFilter mf;
    public MeshRenderer mr;
    public Animator anim;

	private JDictionary laserSpriteData;

    public void Initialize(string classname, Character attacker, Weapon weapon, Vector3 startPos, float angle, float distance, float width, Dictionary<WeaponStats, float> data) {
		laserSpriteData = GameDataManager.instance.RootData[classname]["Sprites"];

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
            Vector3 temp = rayhit.normal;
            temp = Quaternion.AngleAxis(180, Vector3.forward) * temp;
            float ang = Helper.Vector2ToAng(temp);
            if (laserSpriteData["hit"]) {
                EffectManager.instance.CreateEffect(laserSpriteData["hit"].Value<string>(), endPos, ang);
            }
            if (laserSpriteData["hitparticles"]) {
				foreach(JDictionary particle in laserSpriteData["hitparticles"]) {
					ParticleManager.instance.CreateParticle(particle.Value<string>(), endPos, ang, false);
				}
                //Dictionary<string, object> dict = (Dictionary<string, object>)GameDataManager.instance.GetData(classname, "Sprites", "hitparticles");
                //for (int i = 0; i < dict.Count; i++) {
                //    string particleName = (string)dict[i.ToString()];
                //    ParticleManager.instance.CreateParticle(particleName, endPos, ang, false);
                //}
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
        int hitCount = 0;
        foreach(Collider2D col in colliders) {
            Character c = col.GetComponent<Character>();
            if(c != null) {
                if(c.GetTeam() != attacker.GetTeam()) {
                    DamageData dmgData = Helper.DamageCalc(attacker, data, c, true);
                    c.DoDamage(attacker, dmgData.damage, dmgData.stagger);
                    hitCount++;
                }
            }
        }
    }

    IEnumerator DestroyEffect() {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }
}