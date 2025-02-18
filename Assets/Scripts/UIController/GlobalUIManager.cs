using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUIManager : MonoBehaviour {
    public static GlobalUIManager instance;
    public MeshFilter grenadeUImf;
    [HideInInspector]
    public Mesh grenadeUImesh;
    public static Canvas canvas;
    public static int standardHeight = 1080;
    public static int standardWidth = 1920;
    private Dictionary<string, GameObject> UIElement = new Dictionary<string, GameObject>();
    void Awake () {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        canvas = GetComponent<Canvas>();
        DontDestroyOnLoad(this);
        grenadeUImesh = new Mesh();
        grenadeUImf.mesh = grenadeUImesh;
        grenadeUImf.GetComponent<MeshRenderer>().sortingLayerName = "Ground";
    }

    public void LoadScene(string scene) {
        LoadingScreen.instance.Open();
        StartCoroutine(LoadSceneAsync(Convert.ToInt32(scene)));
    }

    IEnumerator LoadSceneAsync(int scene) {
        AsyncOperation load = SceneManager.LoadSceneAsync(scene);
        float lastrun = Time.unscaledTime;
        while (!load.isDone || LoadingScreen.instance.isLoading) {
            if (scene != 2)
                LoadingScreen.instance.Close();
            LoadingScreen.instance.UpdateLoadingAnimation(Time.unscaledTime - lastrun);
            lastrun = Time.unscaledTime;
            yield return null;
        }
    }

    public void BackToMainmenu() {
        LevelManager.instance.DestroyMap();
        PlayerHUD.DestroyUI();
        LoadScene("1");
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

    private void AdjustToScreen(GameObject obj, Vector2 pos, int width = -1, int height = -1) {
        if (!CamController.instance)
            return;
        RectTransform rect = obj.GetComponent<RectTransform>();
        Vector2 screenCorner = new Vector2(Screen.width / 2f, Screen.height / 2f);
        float widthRatio = Screen.width / (float)standardWidth;
        float heightRatio = Screen.height / (float)standardHeight;
        Image img = obj.GetComponent<Image>();
        int _width = width;
        int _height = height;
        if (img != null) {
            Sprite sprite = img.sprite;
            if (width == -1) {
                _width = (int)sprite.rect.size.x;
                _height = (int)sprite.rect.size.y;
            }
            else if(height == -1) {
                _height = (int)(sprite.rect.size.y / sprite.rect.size.x * _width);
            }
        }
        obj.transform.position = pos * new Vector2(widthRatio, heightRatio);
        rect.sizeDelta = new Vector2(_width, _height);
    }

    public void RescaleUI(string uniqueid, float scaleX, float scaleY) {
        if (NullCheck(uniqueid)) return;
        RectTransform rect = UIElement[uniqueid].GetComponent<RectTransform>();
        rect.localScale = new Vector3(scaleX, scaleY, 1);
    }

    public void SetImageTypeFilled(string uniqueid, Image.FillMethod method, int origin, float amount) {
        if (NullCheck(uniqueid)) return;
        Image img = UIElement[uniqueid].GetComponent<Image>();
        img.type = Image.Type.Filled;
        img.fillMethod = method;
        img.fillOrigin = origin;
        img.fillAmount = amount;
    }

    public void SetImageFillAmount(string uniqueid, float amount) {
        if (NullCheck(uniqueid)) return;
        Image img = UIElement[uniqueid].GetComponent<Image>();
        img.fillAmount = amount;
    }

    public void LerpFill(string uniqueid, float from, float to, float duration) {
        if (NullCheck(uniqueid)) return;
        StartCoroutine(LerpFillInternal(uniqueid, from, to, duration));
    }

    IEnumerator LerpFillInternal(string uniqueid, float from, float to, float duration) {
        Image img = UIElement[uniqueid].GetComponent<Image>();
        float elapsed = 0;
        while (elapsed < duration) {
            if (img == null)
                yield break;
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            img.fillAmount = from + (to - from) * percent;
            yield return null;
        }
    }

    public void ChangeText(string uniqueid, string text) {
        if (NullCheck(uniqueid)) return;
        UIElement[uniqueid].GetComponent<Text>().text = text;
    }

    public void SetTextSize(string uniqueid, int size) {
        if (NullCheck(uniqueid)) return;
        UIElement[uniqueid].GetComponent<Text>().fontSize = size;
    }

    public GameObject CreateButton(string uniqueid, string command, string argument, Sprite sprite, Vector2 pos, Transform parent, int width = -1, int height = -1) {
        if (!NullCheck(uniqueid))
            DeleteUIElement(uniqueid);
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Button2D"), pos, new Quaternion(), parent);
        obj.GetComponent<Image>().sprite = sprite;
        AdjustToScreen(obj, pos, width, height);
        obj.GetComponent<GenericButton>().Command = command;
        obj.GetComponent<GenericButton>().Argument = argument;
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateButton(string uniqueid, string command, string argument, Sprite sprite, Vector2 pos, int width = -1, int height = -1) {
        return CreateButton(uniqueid, command, argument, sprite, pos, transform, width, height);
    }

    public GameObject CreateImage(string uniqueid, Sprite sprite, Vector2 pos, Transform parent, int width = -1, int height = -1) {
        if (!NullCheck(uniqueid))
            DeleteUIElement(uniqueid);
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Image2D"), pos, new Quaternion(), parent);
        obj.GetComponent<Image>().sprite = sprite;
        AdjustToScreen(obj, pos, width, height);
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateImage(string uniqueid, Sprite sprite, Vector2 pos, int width = -1, int height = -1) {
        return CreateImage(uniqueid, sprite, pos, transform, width, height);
    }

    public GameObject CreatePanel(string uniqueid, Vector2 pos, Transform parent, int width = -1, int height = -1) {
        return CreateImage(uniqueid, Helper.GetSprite("Sprites/ui/menu", "panel"), pos, parent, width, height);
    }

    public GameObject CreateText(string uniqueid, Vector2 pos, string text, Transform parent, int width = -1, int height = -1, int fontSize = 20) {
        if (!NullCheck(uniqueid))
            DeleteUIElement(uniqueid);
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Text2D"), pos, new Quaternion(), parent);
        obj.GetComponent<Text>().text = text;
        obj.GetComponent<Text>().fontSize = fontSize;
        AdjustToScreen(obj, pos, width, height);
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateText(string uniqueid, Vector2 pos, string text, int width = -1, int height = -1, int fontSize = 20) {
        return CreateText(uniqueid, pos, text, transform, width, height, fontSize);
    }

    public GameObject CreateScrollView(string uniqueid, Vector2 pos, Transform parent, int width = -1, int height = -1) {
        if (!NullCheck(uniqueid))
            DeleteUIElement(uniqueid);
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/ScrollView"), pos, new Quaternion(), parent);
        AdjustToScreen(obj, pos, width, height);
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateImageWorld(string uniqueid, Sprite sprite, Vector3 pos, int width, int height, Transform parent = null) {
        if (!NullCheck(uniqueid)) return null;
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Image"), pos, new Quaternion(), parent);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        obj.GetComponent<Image>().sprite = sprite;
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateLeftToRightBarWorld(string uniqueid, Color color, Vector3 pos, int width, int height, Transform parent = null) {
        if (!NullCheck(uniqueid)) return null;
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Image"), pos, new Quaternion(), parent);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        obj.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
        obj.GetComponent<Image>().color = color;
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateRightToLeftBarWorld(string uniqueid, Color color, Vector3 pos, int width, int height, Transform parent = null) {
        if (!NullCheck(uniqueid)) return null;
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/UI/Image"), pos, new Quaternion(), parent);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        obj.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
        obj.GetComponent<Image>().color = color;
        UIElement.Add(uniqueid, obj);
        return obj;
    }

    public GameObject CreateButtonWorld(string uniqueid, string command, string argument, Sprite sprite, Vector3 pos, int width, int height, Transform parent = null) {
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
        if (!NullCheck(id)) {
            Destroy(UIElement[id]);
            UIElement.Remove(id);
        }
    }

    public Vector2 GetStandardScreenSize() {
        return new Vector2(standardWidth, standardHeight);
    }
}
