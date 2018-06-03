using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    public static LevelManager instance;
    private Dictionary<int, GameObject> createdObjs = new Dictionary<int, GameObject>();
    private Dictionary<int, int> inheritances = new Dictionary<int, int>();
    private List<GameObject> BGParents = new List<GameObject>();
    private Dictionary<string, object> mapdata = null;
    private Vector2 mapMin;
    private Vector2 mapMax;
    private bool loadOnce = false;
    private static string[] essentialSprites = {
        "items",
        "effects",
        "ui"
    };
    public bool debug = false;
	public bool isMapActive = false;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        Cursor.visible = false;
        if (debug)
            StartCoroutine(Debugger());
        else {
            Save.DataLoad();
        }
    }

    IEnumerator Debugger() {
        yield return new WaitForEndOfFrame();
        //LoadMap("map_stage01");
        foreach (GameObject bgparent in GameObject.FindGameObjectsWithTag("BG")) {
            BGParents.Add(bgparent);
        }
        Initialize();
    }

    private void PreloadEssentialSprites() {
        GameObject obj = new GameObject();
        obj.transform.localScale = new Vector3();
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        foreach(string essential in essentialSprites) {
            UnityEngine.Object[] sprites = Resources.LoadAll("Sprites/" + essential, typeof(Sprite));
            foreach (Sprite t in sprites) {
                sr.sprite = t;
            }
        }
        DestroyObject(obj);
        loadOnce = true;
    }

    private void PreloadSprites(string prefix, string classname) {
        GameObject obj = new GameObject();
        obj.transform.localScale = new Vector3();
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        UnityEngine.Object[] sprites = Resources.LoadAll("Sprites/" + prefix + "/" + classname, typeof(Sprite));
        foreach (Sprite t in sprites) {
            sr.sprite = t;
        }
        DestroyObject(obj);
    }

    private void PreloadAudios(string prefix, string classname) {

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
                    float maxX = createdObjs.OrderBy(obj => obj.Value.transform.position.x).Reverse().ToArray()[0].Value.transform.position.x;
                    float minY = createdObjs.Where(obj => obj.Value.transform.position.x == maxX).OrderBy(obj => obj.Value.transform.position.y).ToArray()[0].Value.transform.position.y;
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
            int numOfLayers = Convert.ToInt32(GameDataManager.instance.GetData(mapdata, "NumOfLayers"));
            List<string> BGParentKeys = new List<string>();
            BGParents.Clear();
            for(int i = 0; i < numOfLayers; i++) {
                string key = (string)GameDataManager.instance.GetData(mapdata, "Layer" + i.ToString() + "ID");
                BGParents.Add(CreateEmptyObject(key, basePos));
                BGParentKeys.Add(key);
                BGParents[i].tag = "BG";
            }
            
            foreach (KeyValuePair<string, object> entity in mapdata) {
                GameObject obj = null;
                if (!entity.Value.GetType().Equals(typeof(Dictionary<string, object>))) continue;
                switch((string)GameDataManager.instance.GetData(mapdata, entity.Key, "Tag")) {
                    case "Ground":
                        obj = CreatePrefab("Ground", entity.Key, basePos, true);
                        ApplyComponents(obj, entity.Key);
                        break;
                    case "Ceiling":
                        obj = CreatePrefab("Ceiling", entity.Key, basePos, true);
                        ApplyComponents(obj, entity.Key);
                        break;
                    case "Spawner":
                        obj = CreatePrefab("Spawner", entity.Key, basePos);
                        ApplyComponents(obj, entity.Key);
                        break;
                    case "Light":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        ApplyComponents(obj, entity.Key);
                        break;
                    case "BG_Object":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        ApplyComponents(obj, entity.Key);
                        break;
                    case "BG_Layer":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        ApplyComponents(obj, entity.Key);
                        break;
                    case "BG_Farthest":
                        obj = CreateEmptyObject(entity.Key, basePos);
                        obj.tag = "BG_Farthest";
                        break;
                    case "Trigger":
                        obj = CreatePrefab("Trigger", entity.Key, basePos);
                        ApplyComponents(obj, entity.Key);
                        break;
                }
            }

            foreach(KeyValuePair<int, GameObject> kvp in createdObjs) {
                if (inheritances.ContainsKey(kvp.Key)) {
                    if (createdObjs.ContainsKey(inheritances[kvp.Key])) {
                        kvp.Value.transform.parent = createdObjs[inheritances[kvp.Key]].transform;
                    }
                }
            }
            inheritances.Clear();

            if (!loadOnce)
                PreloadEssentialSprites();
        }
        else {
            Debug.LogError("No such data!");
        }
    }

    private GameObject CreatePrefab(string name, string key, Vector3 basePos, bool snap = false) {
        Vector3 pos = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "X")) + basePos.x
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "Y")) + basePos.y
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Position", "Z")));
        if (snap) {
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            pos.z = Mathf.Round(pos.z);
        }
        Vector3 ang = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "Y"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Rotation", "Z")));
        Vector3 scale = new Vector3(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "Y"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Scale", "Z")));
        GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/" + name), pos, new Quaternion());
        obj.transform.eulerAngles = ang;
        obj.transform.localScale = scale;
        createdObjs.Add(Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "ID")), obj);
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
        createdObjs.Add(Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "ID")), obj);
        return obj;
    }

    private void ApplyComponents(GameObject obj, string key) {
        if (GameDataManager.instance.GetData(mapdata, key, "Sprite") != null)
            ApplySpriteRenderer(obj, key);
        if (GameDataManager.instance.GetData(mapdata, key, "Controller") != null)
            ApplyAnimator(obj, key);
        if (GameDataManager.instance.GetData(mapdata, key, "Spawner") != null)
            ApplySpawner(obj, key);
        if (GameDataManager.instance.GetData(mapdata, key, "Trigger") != null)
            ApplyTrigger(obj, key);
        if (GameDataManager.instance.GetData(mapdata, key, "BoxCollider") != null)
            ApplyBoxCollider(obj, key);
        if (GameDataManager.instance.GetData(mapdata, key, "Light") != null)
            ApplyLight(obj, key);
        if (GameDataManager.instance.GetData(mapdata, key, "Parent") != null) {
            if(GameDataManager.instance.GetData(mapdata, key, "ParentIsCam") != null) {
                inheritances.Add(Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "ID")),
                Camera.main.GetInstanceID());
            }
            else {
                inheritances.Add(Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "ID")),
                Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Parent")));
            }
        }
    }

    private void ApplySpriteRenderer(GameObject obj, string key) {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = Helper.GetSprite((string)GameDataManager.instance.GetData(mapdata, key, "Sprite", "SpritePath"),
                                                                (string)GameDataManager.instance.GetData(mapdata, key, "Sprite", "SpriteName"));
        sr.flipX = Convert.ToBoolean(GameDataManager.instance.GetData(mapdata, key, "Sprite", "FlipX"));
        sr.flipY = Convert.ToBoolean(GameDataManager.instance.GetData(mapdata, key, "Sprite", "FlipY"));
        sr.material = Helper.GetMaterial((string)GameDataManager.instance.GetData(mapdata, key, "Sprite", "MaterialPath"),
            (string)GameDataManager.instance.GetData(mapdata, key, "Sprite", "MaterialName"));
        sr.sortingLayerName = (string)GameDataManager.instance.GetData(mapdata, key, "Sprite", "SortingLayer");
        sr.sortingOrder = Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Sprite", "SortOrder"));
    }

    private void ApplyAnimator(GameObject obj, string key) {
        Animator anim = obj.GetComponent<Animator>();
        if (anim == null)
            anim = obj.AddComponent<Animator>();
        anim.runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load((string)GameDataManager.instance.GetData(mapdata, key, "Controller"));
    }

    private void ApplySpawner(GameObject obj, string key) {
        CharacterSpawner cs = obj.GetComponent<CharacterSpawner>();
        if (cs == null)
            cs = obj.AddComponent<CharacterSpawner>();
        cs.delay = Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Spawner", "Delay"));
        cs.characterClass = (string)GameDataManager.instance.GetData(mapdata, key, "Spawner", "CharacterClass");
        PreloadSprites("characters", cs.characterClass);
        cs.team = (Teams)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Spawner", "Team"));
        cs.weaponClass = (string)GameDataManager.instance.GetData(mapdata, key, "Spawner", "WeaponClass");
        cs.characterType = (CharacterTypes)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Spawner", "CharacterType"));
        cs.spawnerType = (CharacterSpawnerTypes)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Spawner", "SpawnerType"));
    }

    private void ApplyTrigger(GameObject obj, string key) {
        Trigger tr = obj.GetComponent<Trigger>();
        if (tr == null)
            tr = obj.AddComponent<Trigger>();
        tr.action = (string)GameDataManager.instance.GetData(mapdata, key, "Trigger", "TriggerAction");
        List<string> args = new List<string>();
        foreach (KeyValuePair<string, object> arg in (Dictionary<string, object>)GameDataManager.instance.GetData(mapdata, key, "Trigger", "TriggerArguments")) {
            args.Add((string)arg.Value);
        }
        tr.arguments = args.ToArray();
        tr.Initialize();
    }

    private void ApplyBoxCollider(GameObject obj, string key) {
        BoxCollider2D box = obj.GetComponent<BoxCollider2D>();
        if (box == null)
            box = obj.AddComponent<BoxCollider2D>();
        box.offset = new Vector2(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "BoxCollider", "BoxOffset", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "BoxCollider", "BoxOffset", "Y")));
        box.size = new Vector2(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "BoxCollider", "BoxSize", "X"))
                                    , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "BoxCollider", "BoxSize", "Y")));
    }

    private void ApplyLight(GameObject obj, string key) {
        Light light = obj.GetComponent<Light>();
        if (light == null)
            light = obj.AddComponent<Light>();
        light.type = (LightType)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Light", "Type"));
        light.range = Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Light", "Range"));
        light.color = new Color(Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Light", "Color", "R"))
            , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Light", "Color", "G"))
            , Convert.ToSingle(GameDataManager.instance.GetData(mapdata, key, "Light", "Color", "B")));
        light.lightmapBakeType = (LightmapBakeType)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Light", "Mode"));
        light.cookie = Helper.GetTexture((string)GameDataManager.instance.GetData(mapdata, key, "Light", "CookiePath")
            , (string)GameDataManager.instance.GetData(mapdata, key, "Light", "CookieName"));
        light.flare = Helper.GetFlare((string)GameDataManager.instance.GetData(mapdata, key, "Light", "FlarePath"),
            (string)GameDataManager.instance.GetData(mapdata, key, "Light", "FlareName"));
        light.renderMode = (LightRenderMode)Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Light", "RenderMode"));
        light.cullingMask = Convert.ToInt32(GameDataManager.instance.GetData(mapdata, key, "Light", "CullingMask"));
    }

    public void DestroyMap() {
		isMapActive = false;
        if(ScenarySimulation.instance != null)
            ScenarySimulation.instance.OnMapDestroy();
        if(createdObjs.Count > 0) {
            foreach(KeyValuePair<int, GameObject> kvp in createdObjs) {
				if(kvp.Value != null) {
					DestroyObject(kvp.Value);
				}
			}
        }
		createdObjs.Clear();
        if (CharacterManager.instance != null) {
            List<Character> characters = CharacterManager.instance.GetAllCharacters();
            for (int i = characters.Count - 1; i > -1; i--) {
                characters[i].Kill();
            }
        }
		if(EffectManager.instance != null) {
			List<Effect> effects = EffectManager.instance.GetEffects();
			for(int i = 0; i < effects.Count; i++) {
				DestroyObject(effects[i].gameObject);
			}
		}
		if(LootManager.instance != null) {
			List<Loot> loots = LootManager.instance.GetLoots();
			for(int i = 0; i < loots.Count; i++) {
				DestroyObject(loots[i].gameObject);
			}
		}
    }

    public void Initialize() {
		isMapActive = true;
        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float maxY = -Mathf.Infinity;
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
        GetComponent<ScenarySimulation>().Initialize(mapMin, mapMax, BGParents);
        Helper.InitializeLayerMasks();

        foreach (GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
            spawner.GetComponent<CharacterSpawner>().Initialize();
        }

        if (LoadingScreen.instance != null)
            LoadingScreen.instance.Open();
    }

    public Vector2 GetMapMin() {
        return mapMin;
    }

    public Vector2 GetMapMax() {
        return mapMax;
    }


}
