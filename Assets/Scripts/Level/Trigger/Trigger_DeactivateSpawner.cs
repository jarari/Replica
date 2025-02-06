using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_DeactivateSpawner : MonoBehaviour {
    private GameObject target = null;
    public void Initialize(GameObject t) {
        target = t;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject == CharacterManager.GetPlayer()) {
            CharacterSpawner spawner = target.GetComponent<CharacterSpawner>();
            spawner.enabled = false;
            spawner.init = false;
            Destroy(this);
        }
    }
}
