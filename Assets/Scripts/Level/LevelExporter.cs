﻿#if (UNITY_EDITOR)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LevelExporter : MonoBehaviour {
    public bool ScenearySimulation = true;
    private int counter = 0;
    private int BGlayerCount = 0;
    private Dictionary<int, GameObject> BGParents = new Dictionary<int, GameObject>();
    private CharacterSpawner playerspawner;

    public void ExportMap() {
        BGParents.Clear();
        BGlayerCount = 0;
        counter = 0;
        Cursor.visible = true;
        List<GameObject> triggers = new List<GameObject>();
        string data = "{";
        foreach (GameObject obj in FindObjectsOfType<GameObject>()) {
            bool tagged = false;
            Debug.Log("Processing object " + obj.name
            + "\n\tTag: " + obj.tag);
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
                case "Light":
                    tagged = true;
                    data += HandleObject(obj, "Light");
                    counter++;
                    break;
                case "BG_Object":
                    tagged = true;
                    data += HandleObject(obj, "BG_Object");
                    counter++;
                    break;
                case "BG":
                    tagged = true;
                    if (obj.transform.childCount > 0) {
                        int index = 0;
                        SpriteRenderer sr = null;
                        while (sr == null) {
                            sr = obj.transform.GetChild(index).GetComponent<SpriteRenderer>();
                            if (index == obj.transform.childCount && sr == null) {
                                Debug.Log("No objects with SpriteRenderer was found for this background layer!");
                                break;
                            }
                            index++;
                        }
                        int layernum = sr.sortingOrder;
                        BGParents.Add(layernum, obj);
                        data += HandleObject(obj, "BG_Parent");
                        data += "\"Layer" + layernum.ToString() + "ID\":\"" + counter.ToString() + "\",";
                        counter++;
                        BGlayerCount++;
                    }
                    break;
                case "BG_Farthest":
                    tagged = true;
                    data += HandleObject(obj, "BG_Farthest");
                    counter++;
                    break;
                case "WorldText":
                    tagged = true;
                    data += HandleObject(obj, "WorldText");
                    counter++;
                    break;
                case "Trigger":
                    tagged = true;
                    triggers.Add(obj);
                    Debug.Log("Obj was trigger. Object queued.");
                    break;
            }
            if (tagged) continue;
            if (obj.transform.parent != null) {
                if (obj.transform.parent.tag == "BG") {
                    data += HandleObject(obj, "BG_Layer");
                    counter++;
                }
            }
        }
        foreach (GameObject obj in triggers) {
            data += HandleObject(obj, "Trigger");
            counter++;
        }
        data += "\"NumOfLayers\":" + BGlayerCount.ToString() + ",";
        data = data.Substring(0, data.Length - 1) + "}";
        var path = EditorUtility.SaveFilePanel(
                "Save map",
                "",
                "",
                "json");
        if (path.Length != 0)
            System.IO.File.WriteAllText(path, data);
    }
    public void TestRun(bool auto = false) {
        ScenearySimulation = auto;
        StartCoroutine(WaitTillInit());
    }
    IEnumerator WaitTillInit() {
        yield return new WaitWhile(() => CamController.instance == null);

        List<GameObject> BGp = new List<GameObject>(GameObject.FindGameObjectsWithTag("BG"));
        if(playerspawner != null) {
            if (ScenearySimulation) {
                playerspawner.characterType = CharacterTypes.Boss;
                playerspawner.characterClass = "character_dummy";
                playerspawner.weaponClass = "weapon_ai_dummy";
                playerspawner.spawnerType = CharacterSpawnerTypes.Once;
                LevelManager.instance.Initialize();
                CamController.instance.AttachCam(playerspawner.GetLastSpawn().transform);
                playerspawner.GetLastSpawn().SetFlag(CharacterFlags.Invincible);
                playerspawner.GetLastSpawn().SetFlag(CharacterFlags.KnockBackImmunity);
                ScenarySimulation.instance.Initialize(LevelManager.instance.GetMapMin(), LevelManager.instance.GetMapMax(), BGp);
            }
            else {
                LevelManager.instance.Initialize();
            }
        }
    }

    private string GetPathToAsset(UnityEngine.Object asset) {
        string extensionPattern = @"\.[a-zA-Z0-9]*";
        Regex extensionRegex = new Regex(extensionPattern);
        return extensionRegex.Replace(AssetDatabase.GetAssetPath(asset).Replace("Assets/Resources/", ""), "");
    }

    private string HandleObject(GameObject obj, string tag) {
        
        string jsondata = "\"" + counter.ToString()
            + "\":{\"ID\":" + obj.GetInstanceID().ToString()
            + ",\"Name\":\"" + obj.name
            + "\",\"Tag\":\"" + tag
            + "\",\"Layer\":" + obj.layer.ToString()
            + ",\"Position\":{\"X\":" + obj.transform.position.x.ToString()
                            + ",\"Y\":" + obj.transform.position.y.ToString()
                            + ",\"Z\":" + obj.transform.position.z.ToString()
            + "},\"Rotation\":{\"X\":" + obj.transform.eulerAngles.x.ToString()
                            + ",\"Y\":" + obj.transform.eulerAngles.y.ToString()
                            + ",\"Z\":" + obj.transform.eulerAngles.z.ToString()
            + "},\"Scale\":{\"X\":" + obj.transform.localScale.x.ToString()
                            + ",\"Y\":" + obj.transform.localScale.y.ToString()
                            + ",\"Z\":" + obj.transform.localScale.z.ToString()
            + "},";
        if (tag == "WorldText") {
            jsondata += "\"Text\":\"" + obj.GetComponentInChildren<Text>().text + "\",";
        }
        if (obj.transform.parent != null && obj.transform.parent.name != "LevelCreated" && obj.transform.parent.tag != "") {
            if (obj.transform.parent.CompareTag("MainCamera")) {
                jsondata += "\"ParentIsCam\":1,";
            }
            jsondata += "\"Parent\":" + obj.transform.parent.gameObject.GetInstanceID().ToString() + ",";
        }
        if (obj.GetComponent<SpriteRenderer>() != null) {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            jsondata += "\"Sprite\":{"
            + "\"SpritePath\":\"" + GetPathToAsset(sr.sprite)
            + "\",\"SpriteName\":\"" + sr.sprite.name
            + "\",\"FlipX\":" + Convert.ToInt32(sr.flipX).ToString()
            + ",\"FlipY\":" + Convert.ToInt32(sr.flipY).ToString()
            + ",\"MaterialPath\":\"" + GetPathToAsset(sr.sharedMaterial)
            + "\",\"MaterialName\":\"" + sr.sharedMaterial.name
            + "\",\"SortingLayer\":\"" + sr.sortingLayerName
            + "\",\"SortOrder\":" + Convert.ToInt32(sr.sortingOrder).ToString()
            + "},";
        }
        if (obj.GetComponent<Animator>() != null) {
            Animator anim = obj.GetComponent<Animator>();
            jsondata += "\"Controller\":\"" + GetPathToAsset(anim.runtimeAnimatorController)
            + "\",";
        }
        if (obj.GetComponent<CharacterSpawner>() != null) {
            CharacterSpawner cs = obj.GetComponent<CharacterSpawner>();
            jsondata += "\"Spawner\":{"
                + "\"Delay\":" + cs.delay.ToString()
                + ",\"CharacterClass\":\"" + cs.characterClass
                + "\",\"Team\":" + Convert.ToInt32(cs.team).ToString()
                + ",\"WeaponClass\":\"" + cs.weaponClass
                + "\",\"CharacterType\":" + Convert.ToInt32(cs.characterType).ToString()
                + ",\"SpawnerType\":" + Convert.ToInt32(cs.spawnerType).ToString()
                + ",\"FacingRight\":" + cs.facingRight.ToString().ToLower()
                + ",\"AutoEngage\":" + cs.autoEngage.ToString().ToLower()
                + ",\"StartEnabled\":" + cs.startEnabled.ToString().ToLower()
                + "},";
            if (cs.characterType == CharacterTypes.Player)
                playerspawner = cs;
        }
        if(obj.GetComponent<BoxCollider2D>() != null) {
            jsondata += "\"BoxCollider\":{"
                + "\"BoxOffset\":{\"X\":" + obj.GetComponent<BoxCollider2D>().offset.x.ToString()
                            + ",\"Y\":" + obj.GetComponent<BoxCollider2D>().offset.y.ToString()
                + "},\"BoxSize\":{\"X\":" + obj.GetComponent<BoxCollider2D>().size.x.ToString()
                            + ",\"Y\":" + obj.GetComponent<BoxCollider2D>().size.y.ToString()
                + "}"
                + "},";
        }
        if (obj.GetComponent<Trigger>() != null) {
            string[] args = obj.GetComponent<Trigger>().arguments;
            GameObject[] targets = obj.GetComponent<Trigger>().targets;
            jsondata += "\"Trigger\":{"
                + "\"Action\":\"" + obj.GetComponent<Trigger>().action
                + "\",\"Arguments\":{";
            for (int argCount = 0; argCount < args.Length; ++argCount) {
                jsondata += "\"" + argCount.ToString() + "\":\"" + args[argCount] + "\"";
                if (argCount != args.Length - 1)
                    jsondata += ",";
            }
            jsondata += "},\"Targets\":{";
            for (int argCount = 0; argCount < targets.Length; ++argCount) {
                jsondata += "\"" + argCount.ToString() + "\":\"" + targets[argCount].GetInstanceID().ToString() + "\"";
                if (argCount != args.Length - 1)
                    jsondata += ",";
            }
            jsondata += "}},";
        }
        if(obj.GetComponent<Light>() != null) {
            Light light = obj.GetComponent<Light>();
            jsondata += "\"Light\":{"
                + "\"Type\":" + (int)light.type
                + ",\"Intensity\":" + light.intensity
                + ",\"Range\":" + light.range
                + ",\"Color\":{"
                + "\"R\":" + light.color.r + ","
                + "\"G\":" + light.color.g + ","
                + "\"B\":" + light.color.b
                + "}";
            if (light.cookie != null)
                jsondata += ",\"CookiePath\":\"" + GetPathToAsset(light.cookie) + "\""
                    + ",\"CookieName\":\"" + light.cookie.name + "\"";
            if (light.flare != null)
                jsondata += ",\"FlarePath\":\"" + GetPathToAsset(light.flare) + "\""
                    + ",\"FlareName\":\"" + light.flare.name + "\"";
            jsondata += ",\"RenderMode\":" + (int)light.renderMode
                + ",\"CullingMask\":" + light.cullingMask
                + "},";
        }
        return jsondata.Substring(0, jsondata.Length - 1) + "},";
    }

    public void ScenaryToCam() {
        Camera editorCam = SceneView.lastActiveSceneView.camera;
        editorCam.orthographicSize = (GlobalUIManager.standardHeight / (1f * Helper.PixelsPerUnit)) * 0.25f;
        CamController camCon = Camera.main.GetComponent<CamController>();
        camCon.SetupCam(GlobalUIManager.standardWidth, GlobalUIManager.standardHeight);
        Camera.main.transform.position = editorCam.transform.position;

        List<GameObject> BGp = new List<GameObject>(GameObject.FindGameObjectsWithTag("BG"));
        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Ceiling")) {
            minX = Mathf.Min(obj.transform.position.x, minX);
            minY = Mathf.Min(obj.transform.position.y, minY);
            maxX = Mathf.Max(obj.transform.position.x, maxX);
            maxY = Mathf.Max(obj.transform.position.y, maxY);
        }
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Ground")) {
            minX = Mathf.Min(obj.transform.position.x, minX);
            minY = Mathf.Min(obj.transform.position.y, minY);
            maxX = Mathf.Max(obj.transform.position.x, maxX);
            maxY = Mathf.Max(obj.transform.position.y, maxY);
        }
        Vector2 mapMin = new Vector2(minX, minY);
        Vector2 mapMax = new Vector2(maxX, maxY);
        ScenarySimulation sceneSim = GetComponent<ScenarySimulation>();
        sceneSim.Initialize(mapMin, mapMax, BGp);
        sceneSim.UpdateSimulation(editorCam.transform.position, camCon.GetCamSize());
        sceneSim.OnMapDestroy();
    }
}
#endif