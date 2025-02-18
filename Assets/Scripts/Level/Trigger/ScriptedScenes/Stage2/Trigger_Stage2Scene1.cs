using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Stage2Scene1 : MonoBehaviour
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
        CamController cam = CamController.instance;

        player.SetFlag(CharacterFlags.AIControlled);
        targets[0].GetComponent<CharacterSpawner>().GetLastSpawn().SetFlag(CharacterFlags.Invincible);

        player.SetFollow(player.transform.position + Vector3.right * 200f, 10f);
        cam.ZoomCam(1.25f, 1.0f);
        yield return new WaitWhile(() => player.IsFollowing() == true);
        player.FlipFace(true);
        yield return new WaitForSeconds(1.25f);
        player.FlipFace(false);
        yield return new WaitForSeconds(1.25f);
        player.FlipFace(true);
        DialogueManager dm = DialogueManager.instance;
        dm.AddDialogue(player.gameObject, "Where did she go?");
        dm.StartDialogue();
        yield return new WaitForSeconds(1.5f);
        cam.RevertZoom(0.5f);
        player.RemoveFlag(CharacterFlags.AIControlled);
        yield return null;
    }
}
