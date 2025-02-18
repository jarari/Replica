using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GenericButton : MonoBehaviour {
    public string Command;
    public string Argument;

    void Awake() {
        GetComponent<Button>().onClick.AddListener(ExecuteCommand);
    }

    void OnNewGameSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        Save.CreateNewData();
        LevelManager.instance.LoadMap("map_stage01");
        SceneManager.sceneLoaded -= OnNewGameSceneLoaded;
    }

    void ExecuteCommand() {
        switch (Command) {
            case "LoadScene":
                GlobalUIManager.instance.LoadScene(Argument);
                break;
            case "NewGame":
                GlobalUIManager.instance.LoadScene("2");
                SceneManager.sceneLoaded += OnNewGameSceneLoaded;
                break;
            case "ShowMenu":
                MenuManager.instance.ShowMenu(Argument);
                break;
            case "MainMenu":
                GlobalUIManager.instance.StartCoroutine(GlobalUIManager.instance.BackToMainmenu());
                break;
            case "UseItem":
                CharacterManager.GetPlayer().GetInventory().UseItem(Argument, 1);
                break;
            case "ExitGame":
                Application.Quit();
                break;
            case "default":
                break;
        }
    }
}
