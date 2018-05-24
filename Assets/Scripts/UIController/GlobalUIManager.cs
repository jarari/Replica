using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUIManager : MonoBehaviour {
    public static GlobalUIManager instance;
    private Dictionary<string, GameObject> UIElement = new Dictionary<string, GameObject>();
    void Awake () {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    public void LoadScene(int scene) {
        LoadingScreen.instance.Close();
        StartCoroutine(LoadSceneAsync(scene));
    }

    IEnumerator LoadSceneAsync(int scene) {
        AsyncOperation load = SceneManager.LoadSceneAsync(scene);
        while (!load.isDone) {
            if(scene != 2)
                LoadingScreen.instance.Open();
            yield return null;
        }
    }

    private bool NullCheck(string uniqueid) {
        if (UIElement.ContainsKey(uniqueid)) {
            if (UIElement[uniqueid] == null) {
                UIElement.Remove(uniqueid);
                return true;
            }
            return false;
        }
        return true;
    }

    public GameObject CreateImage(string uniqueid, Sprite sprite, Vector3 pos, int width, int height, Transform parent = null) {
        if (!NullCheck(uniqueid)) return null;
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Image"), pos, new Quaternion(), parent);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        obj.GetComponent<Image>().sprite = sprite;
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateLeftToRightBar(string uniqueid, Color color, Vector3 pos, int width, int height, Transform parent = null) {
        if (!NullCheck(uniqueid)) return null;
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Image"), pos, new Quaternion(), parent);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        obj.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
        obj.GetComponent<Image>().color = color;
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateRightToLeftBar(string uniqueid, Color color, Vector3 pos, int width, int height, Transform parent = null) {
        if (!NullCheck(uniqueid)) return null;
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Image"), pos, new Quaternion(), parent);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        obj.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
        obj.GetComponent<Image>().color = color;
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateButton(string uniqueid, string command, int argument, Sprite sprite, Vector3 pos, int width, int height, Transform parent = null) {
        if (!NullCheck(uniqueid)) return null;
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Button"), pos, new Quaternion(), parent);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        obj.GetComponent<Image>().sprite = sprite;
        obj.GetComponent<GenericButton>().Command = command;
        obj.GetComponent<GenericButton>().Argument = argument;
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public void FocusObject(GameObject obj) {
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(obj);
    }

    public GameObject GetCurrentFocus() {
        return EventSystem.current.GetComponent<EventSystem>().currentSelectedGameObject;
    }

    public string GetNameFromUIObject(GameObject obj) {
        foreach(KeyValuePair<string, GameObject> kvp in UIElement) {
            if (kvp.Value == obj)
                return kvp.Key;
        }
        return "";
    }

    public GameObject GetUIObject(string id) {
        if (!NullCheck(id))
            return UIElement[id];
        return null;
    }

    public void ResizeUI(string id, int width, int height) {
        if(!NullCheck(id))
            UIElement[id].GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }

    public void DeleteUIElement(string id) {
        if (!NullCheck(id))
            DestroyObject(UIElement[id]);
    }
}
