using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericButton : MonoBehaviour {
    public string Command;
    public string Argument;

    void Awake() {
        GetComponent<Button>().onClick.AddListener(ExecuteCommand);
    }

    void ExecuteCommand() {
        switch (Command) {
            case "LoadScene":
                GlobalUIManager.instance.LoadScene(Argument);
                break;
            case "ShowMenu":
                MenuManager.instance.ShowMenu(Argument);
                break;
            case "UseItem":
                CharacterManager.GetPlayer().GetInventory().UseItem(Argument, 1);
                break;
            case "default":
                break;
        }
    }
}
