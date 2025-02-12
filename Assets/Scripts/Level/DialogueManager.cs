using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Dialogue {
    public GameObject Speaker { get; set; }
    public string Message { get; set; }
    public Dialogue(GameObject speaker, string message) {
        Speaker = speaker;
        Message = message;
    }
}

public class DialogueManager : MonoBehaviour {
    public static DialogueManager instance;
    public bool IsInDialogue { get; private set; }
    public bool IsPlayingMessage { get; private set; }
    public GameObject CurrentSpeaker { get; private set; }
    public string CurrentMessage { get; private set; }
    private Queue<Dialogue> dialogueQueue;

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        dialogueQueue = new Queue<Dialogue>();
    }

    public void AddDialogue(GameObject speaker, string message) {
        dialogueQueue.Enqueue(new Dialogue(speaker, message));
    }

    public void StartDialogue() {
        if (dialogueQueue.Count == 0) {
            Debug.LogWarning("No dialogue message in queue.");
            return;
        }
        StartCoroutine(ProcessDialogues());
    }

    private IEnumerator ProcessDialogues() {
        IsInDialogue = true;
        while (IsInDialogue) {
            if (dialogueQueue.Count > 0 && !IsPlayingMessage) {
                Dialogue d = dialogueQueue.Dequeue();
                CurrentSpeaker = d.Speaker;
                CurrentMessage = d.Message;
                CurrentSpeaker.GetComponent<CharacterTalk>().ClearText();
                IsPlayingMessage = true;
            }
            else if (IsPlayingMessage) {
                for (int i = 0; i < CurrentMessage.Length; ++i) {
                    CurrentSpeaker.GetComponent<CharacterTalk>().AddText(CurrentMessage[i]);
                    yield return new WaitForSeconds(0.05f);
                }
                IsPlayingMessage = false;
                yield return new WaitForSeconds(Mathf.Max(CurrentMessage.Length / 12f, 1.0f));
            }
            else {
                CurrentSpeaker.GetComponent<CharacterTalk>().ClearText();
                CurrentSpeaker = null;
                CurrentMessage = "";
                IsInDialogue = false;
            }
        }
        yield return null;
    }
}
