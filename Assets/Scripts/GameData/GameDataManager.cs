using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;

public class GameDataManager : MonoBehaviour {
    public static GameDataManager instance;
    public bool debug = false;
    private Dictionary<string, object> GameData;
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
        ParseJSONData("maps/mapdata");
        ParseJSONData("weapons/attackdata");
        ParseJSONData("weapons/bulletdata");
        ParseJSONData("weapons/projectiledata");
        if (!debug)
            GlobalUIManager.instance.LoadScene(1);
    }

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
