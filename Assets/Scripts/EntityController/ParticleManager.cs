using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ParticleManager : MonoBehaviour {
    public static ParticleManager instance;
    private List<GameObject> particles = new List<GameObject>();

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public List<GameObject> GetParticles() {
        return particles;
    }

    private void AddParticles(GameObject obj) {
        particles.Add(obj);
    }

    private void RemoveParticles(GameObject obj) {
        if(particles.Contains(obj))
            particles.Remove(obj);
    }

    public GameObject CreateParticle(string classname, Vector3 pos, float angle, float duration = -1, Transform parent = null, bool flip = false) {
        if (!LevelManager.instance.isMapActive) return null;
        if (GameDataManager.instance.GetData(classname, "Prefab") == null)
            return null;
        GameObject particle_obj = (GameObject)Instantiate(Resources.Load((string)GameDataManager.instance.GetData(classname, "Prefab")), pos, new Quaternion());
        Vector3 ang = particle_obj.transform.eulerAngles;
        ang.z = angle;
        particle_obj.transform.eulerAngles = ang;
        if (parent != null) {
            particle_obj.transform.SetParent(parent);
        }
        float dur = duration;
        if (dur == -1 && GameDataManager.instance.GetData(classname, "Duration") != null)
            dur = Convert.ToSingle(GameDataManager.instance.GetData(classname, "Duration"));
        else if (dur == -1)
            dur = 0.1f;
        if (flip) {
            Vector3 lscale = particle_obj.transform.localScale;
            lscale.x = lscale.x * -1;
            particle_obj.transform.localScale = lscale;
        }
        StartCoroutine(RemoveParticle(particle_obj, dur));
        AddParticles(particle_obj);
        return particle_obj;
    }

    public GameObject CreateParticle(string classname, Vector3 pos, float angle, Transform parent = null, bool flip = false) {
        return CreateParticle(classname, pos, angle, -1, parent, flip);
    }

    public GameObject CreateParticle(string classname, Vector3 pos, float angle, float duration = -1, bool flip = false) {
        return CreateParticle(classname, pos, angle, duration, null, flip);
    }

    public GameObject CreateParticle(string classname, Vector3 pos, float angle, bool flip = false) {
        return CreateParticle(classname, pos, angle, -1, null, flip);
    }

    IEnumerator RemoveParticle(GameObject obj, float dur) {
        yield return new WaitForSeconds(dur);
        DestroyObject(obj);
        RemoveParticles(obj);
    }
}