using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Talk : MonoBehaviour {
    private string[] messages;
    private bool triggered = false;
    public void Initialize(string[] msgs) {
        messages = msgs;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (triggered == false && collider.GetComponentInChildren<Character>() == CharacterManager.GetPlayer()) {
            triggered = true;
            foreach (string msg in messages) {
                DialogueManager.instance.AddDialogue(collider.gameObject, msg);
            }
            DialogueManager.instance.StartDialogue();
            Destroy(this);
        }
    }
}
