using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericButton : MonoBehaviour {
    public string Command;
    public int Argument;

    void Awake() {
        GetComponent<Button>().onClick.AddListener(ExecuteCommand);
    }

    void ExecuteCommand() {
        switch (Command) {
            case "LoadScene":
                GlobalUIManager.instance.LoadScene(Argument);
                break;
            case "ShowMenu":
                PlayerPauseUI.ShowMenu(Argument);
                break;
            case "item_showmenu":
                Inventory.UseItem(Argument);
                break;
            case "default":
                break;
        }
    }
}
