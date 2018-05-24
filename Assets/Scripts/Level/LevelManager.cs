using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    public static LevelManager instance;
    private List<GameObject> createdObjs = new List<GameObject>();
    private Dictionary<string, object> mapdata = null;
    private Vector2 mapMin;
    private Vector2 mapMax;
    public bool debug = false;
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        if (debug)
            StartCoroutine(Debugger());
    }

    IEnumerator Debugger() {
        yield return new WaitForEndOfFrame();
        LoadMap("map_stage01");
    }

    public void LoadMap(string mapname) {
        StartCoroutine(MapCreationSequence(mapname));
    }

    IEnumerator MapCreationSequence(string mapname) {
        if (GameDataManager.instance.GetData("Data", mapname) != null) {
            DestroyMap();
            Dictionary<int, string> mapList = new Dictionary<int, string>();
            int totalNumOfRooms = (int)GameDataManager.instance.GetData("Data", mapname, "TotalNumberOfRooms");
            List<object> Patterns = new List<object>(((Dictionary<string, object>)GameDataManager.instance.GetData("Data", mapname, "Patterns")).Values);
            if (GameDataManager.instance.GetData("Data", mapname, "Reserved") != null) {
                Dictionary<string, object> dict = (Dictionary<string, object>)GameDataManager.instance.GetData("Data", mapname, "Reserved");
                foreach (KeyValuePair<string, object> kvp in dict) {
                    mapList.Add(Convert.ToInt32(kvp.Key), (string)kvp.Value);
                }
            }
            for (int i = 0; i < totalNumOfRooms; i++) {
                if (mapList.ContainsKey(i)) continue;
                int rand = UnityEngine.Random.Range(0, Patterns.Count);
                mapList.Add(i, (string)Patterns[rand]);
                Patterns.RemoveAt(rand);
            }
            for (int i = 0; i < totalNumOfRooms; i++) {
                string pattern = mapList[i];
                if (createdObjs.Count > 0) {
                    float maxX = createdObjs.OrderBy(obj => obj.transform.position.x).Reverse().ToArray()[0].transform.position.x;
                    float minY = createdObjs.Where(obj => obj.transform.position.x == maxX).OrderBy(obj => obj.transform.position.y).ToArray()[0].transform.position.y;
                    CreateMap(pattern, new Vector3(maxX + 32, minY, 0));
                    yield return new WaitForEndOfFrame();
                }
                else {
                    CreateMap(pattern, new Vector3());
                    yield return new WaitForEndOfFrame();
                }
            }
            Initialize();
        }
        else {
            Debug.LogError("No data for the map " + mapname);
            yield break;
        }
    }

    public void CreateMap(string mapname, Vector3 basePos) {
        mapdata = GameDataManager.instance.ParseMapData(mapname);
        if (mapdata != null) {
            GameObject near = null, mid = null, far = null;
            if (GameDataManager.instance.GetData(mapdata, "NearID") != null) {
                near = CreateEmptyObject((string)GameDataManager.instance.GetData(mapdata, "NearID"), basePos);
                near.tag = "BG_Near";
            }

            if (GameDataManager.instance.GetData(mapdata, "MidID") != null) {
                mid = CreateEmptyObject((string)GameDataManager.instance.GetData(mapdata, "MidID"), basePos);
                mid.tag = "BG_Mid";
            }

            if (GameDataManager.instance.GetData(mapdata, "FarID") != null) {
                far = CreateEmptyObject((string)GameDataManager.instance.GetData(mapdata, "FarID"), basePos);
                far.tag = "BG_Far";
            }

            foreach (KeyValuePair<string, object> entity in mapdata) {
                GameObject obj = null;
                if (entity.Key == "NearID" || entity.Key == "MidID" || entity.Key == "FarID") continue;
                switch((string)GameDataManager.instance.GetData(mapdata, entity.Key, "Tag")) {
                    case "Ground":
                        obj = CreatePrefab("Ground", entity.Key, basePos);
                        ApplySpriteRenderer(obj, entity.Key);
                        break;
                    case "Ceiling":
                        obj = CreatePrefab("Ceiling", entity.Key, basePos);
                        ApplySpriteRenderer(obj, entity.Key);
                        break;
                    case "Spawner":
                        obj = CreatePrefab("Spawner", entity.Key, basePos);
                        ApplySpawner(obj, entity.Key);
                        break;
                    case "DirectionalLight":
                        break;
                    case "BG_Object":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        if (GameDataManager.instance.GetData(mapdata, entity.Key, "SpritePath") != null) {
                            obj.AddComponent<SpriteRenderer>();
                            ApplySpriteRenderer(obj, entity.Key);
                        }
                        if (GameDataManager.instance.GetData(mapdata, entity.Key, "Controller") != null) {
                            obj.AddComponent<Animator>();
                            ApplyAnimator(obj, entity.Key);
                        }
                        break;
                    case "BG_Near":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        if(GameDataManager.instance.GetData(mapdata, entity.Key, "SpritePath") != null) {
                            obj.AddComponent<SpriteRenderer>();
                            ApplySpriteRenderer(obj, entity.Key);
                        }
                        if(GameDataManager.instance.GetData(mapdata, entity.Key, "Controller") != null) {
                            obj.AddComponent<Animator>();
                            ApplyAnimator(obj, entity.Key);
                        }
                        obj.transform.parent = near.transform;
                        break;
                    case "BG_Mid":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        if (GameDataManager.instance.GetData(mapdata, entity.Key, "SpritePath") != null) {
                            obj.AddComponent<SpriteRenderer>();
                            ApplySpriteRenderer(obj, entity.Key);
                        }
                        if (GameDataManager.instance.GetData(mapdata, entity.Key, "Controller") != null) {
                            obj.AddComponent<Animator>();
                            ApplyAnimator(obj, entity.Key);
                        }
                        obj.transform.parent = mid.transform;
                        break;
                    case "BG_Far":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        if (GameDataManager.instance.GetData(mapdata, entity.Key, "SpritePath") != null) {
                            obj.AddComponent<SpriteRenderer>();
                            ApplySpriteRenderer(obj, entity.Key);
                        }
                        if (GameDataManager.instance.GetData(mapdata, entity.Key, "Controller") != null) {
                            obj.AddComponent<Animator>();
                            ApplyAnimator(obj, entity.Key);
                        }
                        obj.transform.parent = far.transform;
                        break;
                    case "BG_Farthest":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        obj.tag = "BG_Farthest";
                        break;
                    case "Trigger":
                        obj = CreatePrefab("Trigger", entity.Key, basePos);
                        ApplyTrigger(obj, entity.Key);
                        break;
                }
            }
        }
        else {
            Debug.LogError("No such data!");
        }
    }

    private GameObject CreatePrefab(string name, string key, Vector3 basePos) {
        Vector3 pos = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "X")) + basePos.x
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "Y")) + basePos.y
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "Z")));
        Vector3 ang = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "Y"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "Z")));
        Vector3 scale = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "Y"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "Z")));
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/" + name), pos, new Quaternion());
        obj.transform.eulerAngles = ang;
        obj.transform.localScale = scale;
        createdObjs.Add(obj);
        return obj;
    }

    private GameObject CreateEmptyObject(string key, Vector3 basePos) {
        Vector3 pos = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "X")) + basePos.x
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "Y")) + basePos.y
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "Z")));
        Vector3 ang = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "Y"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "Z")));
        Vector3 scale = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "Y"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "Z")));
        GameObject obj = new GameObject();
        obj.transform.position = pos;
        obj.transform.eulerAngles = ang;
        obj.transform.localScale = scale;
        createdObjs.Add(obj);
        return obj;
    }

    private void ApplySpriteRenderer(GameObject obj, string key) {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        sr.sprite = Helper.GetSprite((string)GameDataManager.instance.GetData(mapdata, key, "SpritePath"),
                                                                (string)GameDataManager.instance.GetData(mapdata, key, "SpriteName"));
        sr.flipX = Convert.ToBoolean(GameDataManager.instance.GetData(mapdata, key, "FlipX"));
        sr.flipY = Convert.ToBoolean(GameDataManager.instance.GetData(mapdata, key, "FlipY"));
        foreach (Material mat in sr.materials) {
            if (mat.name == (string)GameDataManager.instance.GetData(mapdata, key, "Material"))
                sr.material = mat;
        }
        sr.sortingLayerName = (string)GameDataManager.instance.GetData(mapdata, key, "SortingLayer");
        sr.sortingOrder = Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "SortOrder"));
    }

    private void ApplyAnimator(GameObject obj, string key) {
        obj.GetComponent<Animator>().runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load((string)GameDataManager.instance.GetData(mapdata, key, "Controller"));
    }

    private void ApplySpawner(GameObject obj, string key) {
        CharacterSpawner cs = obj.GetComponent<CharacterSpawner>();
        cs.delay = Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Delay"));
        cs.characterClass = (string)GameDataManager.instance.GetData(mapdata, key, "CharacterClass");
        cs.team = (Teams)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Team"));
        cs.weaponClass = (string)GameDataManager.instance.GetData(mapdata, key, "WeaponClass");
        cs.characterType = (CharacterTypes)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "CharacterType"));
        cs.spawnerType = (CharacterSpawnerTypes)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "SpawnerType"));
    }

    private void ApplyTrigger(GameObject obj, string key) {
        Trigger tr = obj.GetComponent<Trigger>();
        tr.action = (string)GameDataManager.instance.GetData(mapdata, key, "TriggerAction");
        List<string> args = new List<string>();
        foreach(KeyValuePair<string, object> arg in (Dictionary<string, object>)GameDataManager.instance.GetData(mapdata, key, "TriggerArguments")) {
            args.Add((string)arg.Value);
        }
        tr.arguments = args.ToArray();
        BoxCollider2D box = obj.GetComponent<BoxCollider2D>();
        box.offset = new Vector2(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "BoxOffset", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "BoxOffset", "Y")));
        box.size = new Vector2(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "BoxSize", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "BoxSize", "Y")));
        tr.Initialize();
    }

    public void DestroyMap() {
        if(createdObjs.Count > 0) {
            for(int i = createdObjs.Count - 1; i > -1; i--) {
                DestroyObject(createdObjs[i]);
            }
        }
        foreach (GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
            spawner.SetActive(false);
        }
        if (CharacterManager.instance != null) {
            List<Character> characters = CharacterManager.instance.GetAllCharacters();
            for (int i = characters.Count - 1; i > -1; i--) {
                characters[i].Kill();
            }
        }
    }

    public void Initialize() {
        float minX = 99999999999;
        float minY = 99999999999;
        float maxX = -99999999999;
        float maxY = -99999999999;
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Ceiling")) {
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
        mapMin = new Vector2(minX, minY);
        mapMax = new Vector2(maxX, maxY);
        GetComponent<ScenarySimulation>().Initialize(mapMin, mapMax);
        if(LoadingScreen.instance != null)
            LoadingScreen.instance.Open();
    }

    public Vector2 GetMapMin() {
        return mapMin;
    }

    public Vector2 GetMapMax() {
        return mapMax;
    }
}
