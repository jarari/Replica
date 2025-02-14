using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Stage2Scene2 : MonoBehaviour
{
    private GameObject[] targets = null;
    private bool triggered = false;
    public void Initialize(GameObject[] t) {
        targets = t;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (triggered == false && collider.GetComponentInChildren<Character>() == CharacterManager.GetPlayer()) {
            triggered = true;
            EventManager.RegisterEvent("GameClear", new EventManager.CharacterDeath(OnCharacterDeath));
            StartCoroutine(Scene2());
        }
    }

    IEnumerator Scene2() {
        Character player = CharacterManager.GetPlayer();
        Character sceneActor = targets[0].GetComponent<CharacterSpawner>().GetLastSpawn();
        CamController cam = CamController.instance;
        DialogueManager dm = DialogueManager.instance;

        player.SetFlag(CharacterFlags.AIControlled);

        cam.InScriptedScene = true;
        Vector3 mid = (player.transform.position + sceneActor.transform.position) / 2;
        cam.SetCamTargetPos(mid);
        cam.ZoomCam(mid, 1.25f, 1.25f);
        yield return new WaitForSeconds(2.0f);
        dm.AddDialogue(player.gameObject, "Security droid?!");
        dm.StartDialogue();
        yield return new WaitForSeconds(3.0f);
        AISecdroid aicon = (AISecdroid)sceneActor.GetController();
        aicon.SetCommand("");
        aicon.SetAIStatus(false);
        sceneActor.SetState(CharacterStates.Idle);
        yield return new WaitForSeconds(1.5f);
        sceneActor.FlipFace(false);
        yield return new WaitForSeconds(1.0f);
        dm.AddDialogue(sceneActor.gameObject, "Criminal detected.");
        dm.StartDialogue();
        yield return new WaitForSeconds(3.0f);
        aicon.ForceAttack(2);
        yield return new WaitForSeconds(0.66f);
        cam.ShakeCam(2.0f, 0.25f);
        yield return new WaitWhile(() => sceneActor.GetState() == CharacterStates.Attack);
        cam.RevertZoom(0.5f);
        cam.InScriptedScene = false;
        player.RemoveFlag(CharacterFlags.AIControlled);
        aicon.SetCommand("Chase");
        aicon.ForceTarget(player);
        aicon.SetAIStatus(true);
        yield return null;
    }

    void OnCharacterDeath(Character character) {
        if (character == targets[0].GetComponent<CharacterSpawner>().GetLastSpawn()) {
            Debug.Log("Game clear");
        }
    }
}
