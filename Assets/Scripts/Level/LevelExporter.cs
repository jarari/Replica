#if (UNITY_EDITOR)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelExporter : MonoBehaviour {
    private int counter = 0;
    private void Awake() {
        string data = "{";
        foreach(GameObject obj in  FindObjectsOfType<GameObject>()) {
            bool tagged = false;
            switch (obj.tag) {
                case "Ground":
                    tagged = true;
                    data += HandleObject(obj, "Ground");
                    counter++;
                    break;
                case "Ceiling":
                    tagged = true;
                    data += HandleObject(obj, "Ceiling");
                    counter++;
                    break;
                case "Spawner":
                    tagged = true;
                    data += HandleObject(obj, "Spawner");
                    counter++;
                    break;
                case "DirectionalLight":
                    tagged = true;
                    data += HandleObject(obj, "DirectionalLight");
                    counter++;
                    break;
                case "BG_Object":
                    tagged = true;
                    data += HandleObject(obj, "BG_Object");
                    counter++;
                    break;
                case "BG_Near":
                    tagged = true;
                    data += HandleObject(obj, "BG_Near_Parent");
                    data += "\"NearID\":\"" + counter.ToString() + "\",";
                    counter++;
                    break;
                case "BG_Mid":
                    tagged = true;
                    data += HandleObject(obj, "BG_Mid_Parent");
                    data += "\"MidID\":\"" + counter.ToString() + "\",";
                    counter++;
                    break;
                case "BG_Far":
                    tagged = true;
                    data += HandleObject(obj, "BG_Far_Parent");
                    data += "\"FarID\":\"" + counter.ToString() + "\",";
                    counter++;
                    break;
                case "BG_Farthest":
                    tagged = true;
                    data += HandleObject(obj, "BG_Farthest");
                    counter++;
                    break;
                case "Trigger":
                    tagged = true;
                    data += HandleObject(obj, "Trigger");
                    counter++;
                    break;
            }
            if (tagged) continue;
            if(obj.transform.parent != null) {
                switch (obj.transform.parent.tag) {
                    case "BG_Near":
                        data += HandleObject(obj, "BG_Near");
                        counter++;
                        break;
                    case "BG_Mid":
                        data += HandleObject(obj, "BG_Mid");
                        counter++;
                        break;
                    case "BG_Far":
                        data += HandleObject(obj, "BG_Far");
                        counter++;
                        break;
                }
            }
        }
        data = data.Substring(0, data.Length - 1) + "}";
        var path = EditorUtility.SaveFilePanel(
                "Save map",
                "",
                "",
                "json");
        if (path.Length != 0)
            System.IO.File.WriteAllText(path, data);
    }

    private string HandleObject(GameObject obj, string tag) {
        string jsondata = "\"" + counter.ToString()
            + "\":{\"ID\":" + obj.GetInstanceID().ToString()
            + ",\"Tag\":\"" + tag
            + "\",\"Position\":{\"X\":" + obj.transform.position.x.ToString()
                            + ",\"Y\":" + obj.transform.position.y.ToString()
                            + ",\"Z\":" + obj.transform.position.z.ToString()
            + "},\"Rotation\":{\"X\":" + obj.transform.eulerAngles.x.ToString()
                            + ",\"Y\":" + obj.transform.eulerAngles.y.ToString()
                            + ",\"Z\":" + obj.transform.eulerAngles.z.ToString()
            + "},\"Scale\":{\"X\":" + obj.transform.localScale.x.ToString()
                            + ",\"Y\":" + obj.transform.localScale.y.ToString()
                            + ",\"Z\":" + obj.transform.localScale.z.ToString()
            + "},";
        Debug.Log("Processing object " + obj.name
            + "\n\tTag: " + tag);
        if (obj.transform.parent != null && obj.transform.parent.name != "LevelCreated")
            jsondata += "\"Parent\":" + obj.transform.parent.gameObject.GetInstanceID().ToString() + ",";
        if (obj.GetComponent<SpriteRenderer>() != null) {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            jsondata += "\"SpritePath\":\"" + AssetDatabase.GetAssetPath(sr.sprite).Replace("Assets/Resources/", "").Replace(".png", "")
            + "\",\"SpriteName\":\"" + sr.sprite.name
            + "\",\"FlipX\":" + Convert.ToInt32(sr.flipX).ToString()
            + ",\"FlipY\":" + Convert.ToInt32(sr.flipY).ToString()
            + ",\"Material\":\"" + sr.material.name
            + "\",\"SortingLayer\":\"" + sr.sortingLayerName
            + "\",\"SortOrder\":" + Convert.ToInt32(sr.sortingOrder).ToString()
            + ",";
        }
        if (obj.GetComponent<Animator>() != null) {
            Animator anim = obj.GetComponent<Animator>();
            jsondata += "\"Controller\":\"" + AssetDatabase.GetAssetPath(anim.runtimeAnimatorController).Replace("Assets/Resources/", "").Replace(".animcontroller", "").Replace(".overrideController", "")
            + "\",";
        }
        if (obj.GetComponent<CharacterSpawner>() != null) {
            CharacterSpawner cs = obj.GetComponent<CharacterSpawner>();
            jsondata += "\"Delay\":" + cs.delay.ToString()
            + ",\"CharacterClass\":\"" + cs.characterClass
            + "\",\"Team\":" + Convert.ToInt32(cs.team).ToString()
            + ",\"WeaponClass\":\"" + cs.weaponClass
            + "\",\"CharacterType\":" + Convert.ToInt32(cs.characterType).ToString()
            + ",\"SpawnerType\":" + Convert.ToInt32(cs.spawnerType).ToString()
            + ",";
        }
        if(obj.GetComponent<BoxCollider2D>() != null) {
            jsondata += "\"BoxOffset\":{\"X\":" + obj.GetComponent<BoxCollider2D>().offset.x.ToString()
                            + ",\"Y\":" + obj.GetComponent<BoxCollider2D>().offset.y.ToString()
            + "},\"BoxSize\":{\"X\":" + obj.GetComponent<BoxCollider2D>().size.x.ToString()
                            + ",\"Y\":" + obj.GetComponent<BoxCollider2D>().size.y.ToString()
            + "},";
        }
        if (obj.GetComponent<Trigger>() != null) {
            string[] args = obj.GetComponent<Trigger>().arguments;
            int triggercount = 0;
            jsondata += "\"TriggerAction\":\"" + obj.GetComponent<Trigger>().action
            + "\",\"TriggerArguments\":{";
            foreach (string arg in args) {
                jsondata += "\"" + triggercount.ToString() + "\":\"" + arg + "\",";
            }
            jsondata = jsondata.Substring(0, jsondata.Length - 1);
            jsondata += "},";
        }
        return jsondata.Substring(0, jsondata.Length - 1) + "},";
    }
}
#endif