using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

[Flags] public enum KeyCombo {
    None = 0x0,
    Left = 0x1,
    Up = 0x10,
    Right = 0x100,
    Down = 0x1000,
    X = 0x10000
}

public class ComboData {
    public List<KeyCombo> keyCombos = new List<KeyCombo>();
    public List<string> nextPossibleCombos = new List<string>();
    public bool isJumpAttack = false;
    public ComboData(List<KeyCombo> c, List<string> n, bool ja = false) {
        keyCombos = c;
        nextPossibleCombos = n;
        isJumpAttack = ja;
    }
}

/* 게임에 쓰일 데이터들을 관리하는 클래스
 * 모든 데이터는 json 형태로 관리하며
 * 이 클래스에서 ParseJSONData로 호출하면 등록됨. */
public class GameDataManager : MonoBehaviour {
    public static GameDataManager instance;
    public bool debug = false;
    private Dictionary<string, object> GameData;
    private Dictionary<string, ComboData> basicCombos;
    private Dictionary<string, ComboData> continuousCombos;
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        Cursor.visible = false;
        DontDestroyOnLoad(this);
        if(LoadingScreen.instance != null)
            LoadingScreen.instance.Close();
        GameData = new Dictionary<string, object>();
        ParseJSONData("characters/main");
        ParseJSONData("weapons/weapondata");
        ParseJSONData("effects/effectdata");
        ParseJSONData("ai/aidata");
        ParseJSONData("items/itemdata");
        ParseJSONData("items/lootdata");
        ParseJSONData("maps/mapdata");
        ParseJSONData("weapons/attackdata");
        ParseJSONData("weapons/bulletdata");
        ParseJSONData("weapons/projectiledata");
        if (!debug)
            GlobalUIManager.instance.LoadScene(1);
    }

    private void ParseComboData(Dictionary<string, object> data) {
        basicCombos = new Dictionary<string, ComboData>();
        continuousCombos = new Dictionary<string, ComboData>();
        foreach(string key in data.Keys) {
            if(GetData(data, key, "Combo") != null) {
                List<KeyCombo> keyCombos = new List<KeyCombo>();
                List<string> nextPossibleCombos = new List<string>();
                bool isBasic = false;
                bool isJumpAttack = false;
                if (GetData(data, key, "Combo", "IsBasic") != null && Convert.ToSingle(GetData(data, key, "Combo", "IsBasic")) == 1)
                    isBasic = true;
                if (GetData(data, key, "Combo", "IsJumpAttack") != null && Convert.ToSingle(GetData(data, key, "Combo", "IsJumpAttack")) == 1)
                    isJumpAttack = true;
                foreach (string keycombo in ((Dictionary<string, object>)GetData(data, key, "Combo", "Keys")).Values) {
                    switch (keycombo) {
                        case "Left":
                            keyCombos.Add(KeyCombo.Left);
                            break;
                        case "Up":
                            keyCombos.Add(KeyCombo.Up);
                            break;
                        case "Right":
                            keyCombos.Add(KeyCombo.Right);
                            break;
                        case "Down":
                            keyCombos.Add(KeyCombo.Down);
                            break;
                        case "LeftUp":
                            keyCombos.Add(KeyCombo.Left | KeyCombo.Up);
                            break;
                        case "RightUp":
                            keyCombos.Add(KeyCombo.Right | KeyCombo.Up);
                            break;
                        case "LeftDown":
                            keyCombos.Add(KeyCombo.Left | KeyCombo.Down);
                            break;
                        case "RightDown":
                            keyCombos.Add(KeyCombo.Right | KeyCombo.Down);
                            break;
                        case "X":
                            keyCombos.Add(KeyCombo.X);
                            break;
                    }
                }
                if (GetData(data, key, "Combo", "PossibleCombos") != null) {
                    foreach (string nextcombo in ((Dictionary<string, object>)GetData(data, key, "Combo", "PossibleCombos")).Values) {
                        nextPossibleCombos.Add(nextcombo);
                    }
                }
                if (isBasic) {
                    basicCombos.Add(key, new ComboData(keyCombos, nextPossibleCombos, isJumpAttack));
                }
                else {
                    continuousCombos.Add(key, new ComboData(keyCombos, nextPossibleCombos, isJumpAttack));
                }
            }
        }
        //GameData.Add("BasicCombos", basicCombos);
        //GameData.Add("ContinuousCombos", continuousCombos);
    }

    /* JSON 안의 모든 내용을 찾아서 Dictionary화 시키는 함수.
     * 자바스크립트의 object와 비슷한 형태가 됨. */
    private Dictionary<string, object> RecursiveDigger(Dictionary<string, object> target) {
        Dictionary<string, object> digged = new Dictionary<string, object>();
        foreach(KeyValuePair<string, object> obj in target) {
            if(obj.Value.GetType().Equals(typeof(JObject))) {
                digged.Add(obj.Key, RecursiveDigger(((JObject)obj.Value).ToObject<Dictionary<string, object>>()));
            }
            else {
                object temp = obj.Value;
                if (obj.Value.GetType().Equals(typeof(System.Int64)))
                    temp = Convert.ToInt32(obj.Value);
                else if (obj.Value.GetType().Equals(typeof(double)))
                    temp = Convert.ToSingle(obj.Value);
                digged.Add(obj.Key, temp);
            }
        }
        return digged;
    }

    private void ParseJSONData(string filename) {
        TextAsset txtdata = Resources.Load<TextAsset>("Data/" + filename);
        if (txtdata != null) {
            string dataAsJson = txtdata.text;
            JObject jsonObj = JObject.Parse(dataAsJson);
            Dictionary<string, object> WholeFile = jsonObj.ToObject<Dictionary<string, object>>();

            if (GameData.ContainsKey("Data")) {
                Dictionary<string, object> temp = RecursiveDigger(WholeFile);
                if (filename == "weapons/attackdata")
                    ParseComboData(temp);
                Dictionary<string, object> data = (Dictionary<string, object>)GameData["Data"];
                foreach (KeyValuePair<string, object> kvp in temp) {
                    data.Add(kvp.Key, kvp.Value);
                }
                GameData.Clear();
                GameData.Add("Data", data);
            }
            else {
                GameData.Add("Data", RecursiveDigger(WholeFile));
            }
        }
        else {
            Debug.LogError("Cannot find file!");
        }
    }

    public Dictionary<string, object> ParseMapData(string mapname) {
        TextAsset txtdata = Resources.Load<TextAsset>("Data/maps/" + mapname);
        if (txtdata != null) {
            string dataAsJson = txtdata.text;
            JObject jsonObj = JObject.Parse(dataAsJson);
            Dictionary<string, object> WholeFile = jsonObj.ToObject<Dictionary<string, object>>();

            return RecursiveDigger(WholeFile);
        }
        else {
            Debug.LogError("Cannot find file!");
        }
        return null;
    }

    public object GetData(params string[] Keys) {
        Dictionary<string, object> tempDict = GameData;
        for(int i = 0; i < Keys.Length - 1; i++) {
            if (tempDict.ContainsKey(Keys[i])) {
                tempDict = (Dictionary<string, object>)tempDict[Keys[i]];
            }
            else {
                //Debug.LogError("GameData : Invalid key - " + Keys[i]);
                return null;
            }
        }
        if(tempDict.ContainsKey(Keys[Keys.Length - 1]))
            return tempDict[Keys[Keys.Length - 1]];
        return null;
    }

    /* 이 매니저가 관리하지 않는 데이터 (예: 맵데이터는 레벨 매니저가 관리) 를 읽어올 때 사용 */
    public object GetData(Dictionary<string, object> data, params string[] Keys) {
        Dictionary<string, object> tempDict = data;
        for (int i = 0; i < Keys.Length - 1; i++) {
            if (tempDict.ContainsKey(Keys[i])) {
                tempDict = (Dictionary<string, object>)tempDict[Keys[i]];
            }
            else {
                Debug.LogError("GameData : Invalid key - " + Keys[i]);
                return null;
            }
        }
        if (tempDict.ContainsKey(Keys[Keys.Length - 1]))
            return tempDict[Keys[Keys.Length - 1]];
        return null;
    }

    public Dictionary<string, ComboData> GetBasicComboData() {
        return basicCombos;
    }
    public Dictionary<string, ComboData> GetContinuousComboData() {
        return continuousCombos;
    }

    /* 빠른 접근을 위한 스탯 관련 함수들 */
    public float GetCharacterStat(string classname, CharacterStats stat) {
        switch (stat) {
            case CharacterStats.Health:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "Health"));
            case CharacterStats.MoveSpeed:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "MoveSpeed"));
            case CharacterStats.JumpPower:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "JumpPower"));
            case CharacterStats.MeleeArmor:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "MeleeArmor"));
            case CharacterStats.RangeArmor:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "RangeArmor"));
            case CharacterStats.GrenadeFullCharge:
                if(GetData("Data", classname, "Stats", "GrenadeFullCharge") != null)
                    return Convert.ToSingle(GetData("Data", classname, "Stats", "GrenadeFullCharge"));
                return 2f;
            case CharacterStats.GrenadeThrowPower:
                if (GetData("Data", classname, "Stats", "GrenadeThrowPower") != null)
                    return Convert.ToSingle(GetData("Data", classname, "Stats", "GrenadeThrowPower"));
                return 600f;
        }
        return -1;
    }

    public float GetWeaponStat(string classname, WeaponStats stat) {
        switch (stat) {
            case WeaponStats.AttackSpeed:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "AttackSpeed"));
            case WeaponStats.Damage:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "Damage"));
            case WeaponStats.MagSize:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "MagSize"));
            case WeaponStats.ReloadTime:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "ReloadTime"));
            case WeaponStats.SADestruction:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "SADestruction"));
            case WeaponStats.Spread:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "Spread"));
            case WeaponStats.Stagger:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "Stagger"));
            case WeaponStats.Range:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "Range"));
            case WeaponStats.BulletSpeed:
                return Convert.ToSingle(GetData("Data", classname, "Stats", "BulletSpeed"));
        }
        return -1;
    }

    /* 애니메이터 컨트롤러를 가져오는 함수.
     * JSON 데이터에 컨트롤러가 여러개 적혀있는 경우가 있는데,
     * 이 경우 그 컨트롤러 중 하나를 랜덤하게 가져옴.
     * 주로 총알 피격이펙트 랜덤 재생에 사용됨. */
    public RuntimeAnimatorController GetAnimatorController(string classname) {
        string controllername;
        if (GetData("Data", classname, "Sprites", "controller").GetType().Equals(typeof(Dictionary<string, object>))) {
            Dictionary<string, object> dict = (Dictionary<string, object>)GetData("Data", classname, "Sprites", "controller");
            int rand = UnityEngine.Random.Range(0, dict.Count);
            controllername = (string)dict[rand.ToString()];
        }
        else {
            controllername = (string)GetData("Data", classname, "Sprites", "controller");
        }
        var controller = Resources.Load("AnimatorController/" + classname + "/" + controllername);
        if (controller == null)
            return null;
        if (controller.GetType().Equals(typeof(AnimatorOverrideController)))
            return (AnimatorOverrideController)controller;
        return (RuntimeAnimatorController)controller;
    }
}
