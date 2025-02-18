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

        player.SetFlag(CharacterFlags.AIControlled | CharacterFlags.Invincible);
        
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
        player.RemoveFlag(CharacterFlags.AIControlled | CharacterFlags.Invincible);
        aicon.SetCommand("Chase");
        aicon.ForceTarget(player);
        aicon.SetAIStatus(true);
        yield return null;
    }

    void OnCharacterDeath(Character character) {
        if (character == targets[0].GetComponent<CharacterSpawner>().GetLastSpawn()) {
            EventManager.UnregisterEvent("GameClear");
            StartCoroutine(Scene3());
        }
    }

    IEnumerator Scene3() {
        Character player = CharacterManager.GetPlayer();
        CamController cam = CamController.instance;
        DialogueManager dm = DialogueManager.instance;
        GlobalUIManager ui = GlobalUIManager.instance;
        MenuManager mm = MenuManager.instance;
        targets[1].GetComponent<BoxCollider2D>().enabled = false;
        targets[1].SetActive(false);
        mm.CanShowMenu = false;
        player.SetFlag(CharacterFlags.AIControlled);
        cam.ZoomCam(1.25f, 1.25f);
        player.SetFollow(targets[2].transform.position, 10f);
        yield return new WaitWhile(() => player.IsFollowing() == true);
        dm.AddDialogue(player.gameObject, "I have no idea what's going on here.");
        dm.StartDialogue();
        yield return new WaitForSeconds(2.0f);
        dm.AddDialogue(player.gameObject, ".........");
        dm.AddDialogue(player.gameObject, "Whatever. I should get to the HQ.");
        dm.StartDialogue();
        yield return new WaitForSeconds(7.0f);
        player.SetFollow(player.transform.position + Vector3.right * 10000f, 1f);
        yield return new WaitForSeconds(1.75f);
        ui.CreateImage("BGBlack", Helper.GetSprite("Sprites/ui/splashscreen", "BGBlack"), new Vector2(960, 540), Screen.width, Screen.height);
        ui.CreateText("ToBeContinued", new Vector2(960, 540), "To be continued.", 100, 100, 84);
        yield return new WaitForSeconds(3.0f);
        ui.DeleteUIElement("BGBlack");
        ui.DeleteUIElement("ToBeContinued");
        mm.CanShowMenu = true;
        ui.StartCoroutine(ui.BackToMainmenu());
        yield return null;
    }
}
