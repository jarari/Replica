using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_ActivateSpawner : MonoBehaviour {
    public GameObject target = null;
    public void Initialize(GameObject t) {
        target = t;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.GetComponentInChildren<Character>() == CharacterManager.GetPlayer()) {
            CharacterSpawner spawner = target.GetComponent<CharacterSpawner>();
            spawner.enabled = true;
            spawner.init = true;
            if (spawner.spawnerType == CharacterSpawnerTypes.Once || spawner.spawnerType == CharacterSpawnerTypes.OnDeath)
                spawner.Spawn();
            Destroy(this);
        }
    }
}
