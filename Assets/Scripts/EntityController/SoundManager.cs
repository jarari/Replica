using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 사운드 재생 클래스
 * 오브젝트에 부착된 사운드, 오브젝트와 독립된 사운드 재생 가능 */
public class SoundManager : MonoBehaviour {
    public static SoundManager instance;
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void RequestSound(GameObject obj, string path, float vol = 1f, float pitch = 1f, bool loop = false) {
        AudioSource As = obj.AddComponent<AudioSource>();
        if (Resources.Load(path) == null) return;
        As.clip = (AudioClip)Resources.Load(path);
        As.volume = vol;
        As.pitch = pitch;
        if (loop)
            As.loop = loop;
        else
            StartCoroutine(DeleteSound(As, As.clip.length));
        As.Play();
    }

    IEnumerator DeleteSound(AudioSource As, float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(As);
    }

    public void RequestObjectIndependentSound(Vector3 pos, string path, float vol = 1f, float pitch = 1f, bool loop = false) {
        GameObject obj = new GameObject();
        obj.transform.position = pos;
        AudioSource As = obj.AddComponent<AudioSource>();
        if (Resources.Load(path) == null) return;
        As.clip = (AudioClip)Resources.Load(path);
        As.volume = vol;
        As.pitch = pitch;
        if (loop)
            As.loop = loop;
        else
            StartCoroutine(DeleteAll(obj, As, As.clip.length));
        As.Play();
    }

    IEnumerator DeleteAll(GameObject obj, AudioSource As, float delay) {
        yield return new WaitForSeconds(delay);
        DestroyObject(obj);
        Destroy(As);
    }
}
