using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour {
    public static LoadingScreen instance;
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    public void Close() {
        gameObject.SetActive(true);
    }

    public void Open() {
        gameObject.SetActive(false);
    }
}
