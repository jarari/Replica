using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_LoadStage : MonoBehaviour {
    private string nextStage = "";
	public void Initialize(string next) {
        nextStage = next;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (!collider.GetComponentInChildren<Character>().IsAI()) {
            LoadingScreen.instance.Close();
            Save.SaveData();
            StartCoroutine(WaitForLoadingScreen());
        }
    }

    IEnumerator WaitForLoadingScreen() {
        yield return new WaitWhile(() => !LoadingScreen.instance.gameObject.activeInHierarchy);
        LevelManager.instance.LoadMap(nextStage);
    }
}
