using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_ActivateSpawner : MonoBehaviour {
    private GameObject[] targets = null;
    private bool triggered = false;
    public void Initialize(GameObject[] t) {
        targets = t;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (triggered == false && collider.GetComponentInChildren<Character>() == CharacterManager.GetPlayer()) {
            triggered = true;
            foreach (GameObject target in targets) {
                CharacterSpawner spawner = target.GetComponent<CharacterSpawner>();
                spawner.enabled = true;
                if (spawner.spawnerType == CharacterSpawnerTypes.Once || spawner.spawnerType == CharacterSpawnerTypes.OnDeath)
                    spawner.Spawn();
                spawner.init = true;
            }
            Destroy(this);
        }
    }
}
