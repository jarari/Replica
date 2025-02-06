using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour {
    public static LoadingScreen instance;
    public bool isLoading = false;
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    public void Open() {
        gameObject.SetActive(true);
        isLoading = true;
    }

    public void Close() {
        gameObject.SetActive(false);
        isLoading = false;
    }
}
