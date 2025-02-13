using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Stage1Scene1 : MonoBehaviour
{
    private GameObject[] targets = null;
    private bool triggered = false;
    public void Initialize(GameObject[] t) {
        targets = t;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (triggered == false && collider.GetComponentInChildren<Character>() == CharacterManager.GetPlayer()) {
            triggered = true;
            StartCoroutine(Scene1());
        }
    }

    IEnumerator Scene1() {
        Character player = CharacterManager.GetPlayer();
        Character sceneActor = targets[0].GetComponent<CharacterSpawner>().GetLastSpawn();
        CamController cam = CamController.instance;

        player.SetFlag(CharacterFlags.AIControlled);

        cam.InScriptedScene = true;
        Vector3 mid = (player.transform.position + sceneActor.transform.position) / 2;
        cam.SetCamTargetPos(mid);
        cam.ZoomCam(sceneActor.transform.position, 1.5f, 2.0f);
        yield return new WaitForSeconds(2.0f);
        cam.ZoomCam(mid, 1.25f, 1.25f);
        yield return new WaitForSeconds(1.25f);
        DialogueManager dm = DialogueManager.instance;
        dm.AddDialogue(player.gameObject, "That's... me?");
        dm.AddDialogue(sceneActor.gameObject, ".......");
        dm.StartDialogue();
        yield return new WaitForSeconds(2.5f);
        sceneActor.SetFollow(sceneActor.transform.position + Vector3.right * 1000f, 1f);
        yield return new WaitForSeconds(0.5f);
        dm.AddDialogue(player.gameObject, "Wait!");
        dm.StartDialogue();
        yield return new WaitForSeconds(1.0f);
        cam.ZoomCam(player.transform.position, 1.0f, 0.5f);
        cam.InScriptedScene = false;
        player.RemoveFlag(CharacterFlags.AIControlled);
        yield return null;
    }
}
