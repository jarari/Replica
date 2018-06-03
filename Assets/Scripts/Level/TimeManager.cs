using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {
    private static int currentPriority = 0;
    public static TimeManager instance;
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }
    public IEnumerator ChangeTimeScale(float scale, float duration, int priority) {
        if (priority < currentPriority) yield break;
        Time.timeScale = scale;
        float endTime = Time.realtimeSinceStartup + duration;
        float lastrun = Time.realtimeSinceStartup;
        currentPriority = priority;
        while (endTime > Time.realtimeSinceStartup && priority == currentPriority) {
            if (MenuManager.instance.IsPaused()) {
                endTime += Time.realtimeSinceStartup - lastrun;
            }
            lastrun = Time.realtimeSinceStartup;
            yield return null;
        }
        Time.timeScale = 1f;
        currentPriority = 0;
    }

    private IEnumerator PauseTime() {
        float before = Time.timeScale;
        Time.timeScale = 0;
        yield return new WaitWhile(() => MenuManager.instance.IsPaused());
        Time.timeScale = before;
    }

    public void Pause() {
        StartCoroutine(PauseTime());
    }
}
