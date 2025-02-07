using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_LoadStage : MonoBehaviour {
    private string nextStage = "";
    private bool triggered = false;
	public void Initialize(string next) {
        nextStage = next;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (triggered == false && !collider.GetComponentInChildren<Character>().IsAI()) {
            triggered = true;
            LoadingScreen.instance.Open();
            Save.SaveData();
            StartCoroutine(WaitForLoadingScreen());
        }
    }

    IEnumerator WaitForLoadingScreen() {
        yield return new WaitWhile(() => !LoadingScreen.instance.gameObject.activeInHierarchy);
        LevelManager.instance.LoadMap(nextStage);
    }
}
