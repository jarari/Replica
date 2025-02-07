using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_DeactivateSpawner : MonoBehaviour {
    private GameObject target = null;
    private bool triggered = false;
    public void Initialize(GameObject t) {
        target = t;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (triggered == false && collider.gameObject == CharacterManager.GetPlayer()) {
            triggered = true;
            CharacterSpawner spawner = target.GetComponent<CharacterSpawner>();
            spawner.enabled = false;
            spawner.init = false;
            Destroy(this);
        }
    }
}
