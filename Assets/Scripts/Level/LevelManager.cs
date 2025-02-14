using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {
    public static LevelManager instance;
    private Dictionary<int, GameObject> createdObjs = new Dictionary<int, GameObject>();
    private Dictionary<int, int> inheritances = new Dictionary<int, int>();
    private List<GameObject> BGParents = new List<GameObject>();

	private Dictionary<string, object> mapdata = null;
	private JDictionary stageData;

    private Vector2 mapMin;
    private Vector2 mapMax;
    private bool loadOnce = false;
    private static string[] essentialSprites = {
        "items",
        "effects",
        "ui"
    };
    private static int lastBlockID = 0;
    private string currentMap = "";
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
    }

    IEnumerator Debugger() {
        yield return new WaitForEndOfFrame();
        //LoadMap("map_stage01");
        isMapActive = true;
        foreach (GameObject bgparent in GameObject.FindGameObjectsWithTag("BG")) {
            BGParents.Add(bgparent);
        }
        Initialize();
        foreach (GameObject trigger in GameObject.FindGameObjectsWithTag("Trigger")) {
            trigger.GetComponent<Trigger>().Initialize();
        }
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
        Destroy(obj);
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
        Destroy(obj);
    }

    private void PreloadAudios(string prefix, string classname) {

    }

    public void LoadMap(string mapname) {
        StartCoroutine(MapCreationSequence(mapname));
    }

    IEnumerator MapCreationSequence(string mapname) {
		JDictionary mapData = GameDataManager.instance.RootData[mapname];

        if (mapData) {
            currentMap = mapname;
            DestroyMap();
			int totalNumOfRooms = mapData["TotalNumberOfRooms"].Value<int>();

			List<JDictionary> mapList = new List<JDictionary>();
			List<JDictionary> patterns = new List<JDictionary>();

			if(mapData["Patterns"])
				foreach(JDictionary map in mapData["Patterns"])
					patterns.Add(map);

			if (mapData["Reserved"])
				foreach(JDictionary map in mapData["Reserved"])
					mapList.Add(map);

			// mapList의 count가 totalNumOfRooms가 될 때까지 패턴들 중 임의로 하나 선택 후 mapList에 추가.
			// 중복 선택 방지로 선택 후 patterns에서 지운다.
			int mapListCount = mapList.Count;
			for(int i = 0; i < totalNumOfRooms - mapListCount; i++) {
				int randomKey = UnityEngine.Random.Range(0, patterns.Count);
				mapList.Add(patterns[randomKey]);
				patterns.RemoveAt(randomKey);
			}

			for(int i = 0; i < totalNumOfRooms; i++) {
                string pattern = mapList[i].Value<string>();
                if (createdObjs.Count > 0) {
                    float maxX = createdObjs.OrderBy(obj => obj.Value.transform.position.x).Reverse().ToArray()[0].Value.transform.position.x;
                    float minY = createdObjs.Where(obj => obj.Value.transform.position.x == maxX).OrderBy(obj => obj.Value.transform.position.y).ToArray()[0].Value.transform.position.y;
                    lastBlockID = createdObjs.Count;
                    CreateMap(pattern, new Vector3(maxX + 32, minY, 0));
                    yield return new WaitForEndOfFrame();
                }
                else {
                    CreateMap(pattern, new Vector3());
                    yield return new WaitForEndOfFrame();
                }
            }
            isMapActive = true;
            Initialize();
            EventManager.OnMapCreated();
            if (!loadOnce)
                PreloadEssentialSprites();
        }
        else {
            Debug.LogError("No data for the map " + mapname);
            yield break;
        }
    }

	public void CreateMap(string mapname, Vector3 basePos) {
		// Stage data initialize
		stageData = new JDictionary();
		stageData.DeserializeJson("Data/maps/" + mapname);

		if(stageData) {
            int numOfLayers = 0;
            if (stageData["NumOfLayers"])
                numOfLayers = stageData["NumOfLayers"].Value<int>();

			// Backgrounds
			this.BGParents.Clear();
			List<string> BGParentKeys = new List<string>();
			for(int i = 0; i < numOfLayers; i++) {
				string key = stageData["Layer" + i.ToString() + "ID"].Value<string>();

				BGParents.Add(CreateEmptyObject(key, basePos));
				BGParentKeys.Add(key);
				BGParents[i].tag = "BG";
                BGParents[i].name = stageData[key]["Name"].Value<string>();
			}

			foreach(JDictionary entity in stageData) {
				GameObject obj = null;

				if(entity.IsValue)
					continue;

				switch(entity["Tag"].Value<string>()) {
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
                    case "WorldText":
                        obj = CreatePrefab("WorldText", entity.Key, basePos);
                        ApplyComponents(obj, entity.Key);
                        break;
					case "Trigger":
						obj = CreatePrefab("Trigger", entity.Key, basePos);
						ApplyComponents(obj, entity.Key);
						break;
				}
			}

			foreach(KeyValuePair<int, GameObject> kvp in createdObjs) {
				if(inheritances.ContainsKey(kvp.Key)) {
                    if (createdObjs.ContainsKey(inheritances[kvp.Key])) {
						kvp.Value.transform.parent = createdObjs[inheritances[kvp.Key]].transform;
					}
				}
			}
			inheritances.Clear();
		}
		else {
			Debug.LogError("No such data!");
		}
	}

    private int GetBlockObjectID(int dataID) {
        return dataID + lastBlockID;
    }

    private int GetParentBlockObjectID(int uniqueID) {
        foreach (JDictionary entity in stageData) {
            if (entity.IsValue)
                continue;

            if (entity["ID"] && entity["ID"].Value<int>() == uniqueID)
                return int.Parse(entity.Key) + lastBlockID;
        }
        return -1;
    }

    private GameObject GetBlockObjectByID(int instaceID) {
        foreach (JDictionary entity in stageData) {
            if (entity.IsValue)
                continue;

            if (entity["ID"] && entity["ID"].Value<int>() == instaceID) {
                GameObject ret;
                createdObjs.TryGetValue(int.Parse(entity.Key) + lastBlockID, out ret);
                return ret;
            }
        }
        return null;
    }

    private GameObject CreatePrefab(string name, string key, Vector3 basePos, bool snap = false) {
		GameObject obj = (GameObject) Instantiate(Resources.Load("Prefab/" + name));

		if(snap) {
			obj.transform.position = new Vector3(
				Mathf.Round(this.stageData[key]["Position"]["X"].Value<float>() + basePos.x),
				Mathf.Round(this.stageData[key]["Position"]["Y"].Value<float>() + basePos.y),
				Mathf.Round(this.stageData[key]["Position"]["Z"].Value<float>())
				);
		}
		else {
			obj.transform.position = new Vector3(
				this.stageData[key]["Position"]["X"].Value<float>() + basePos.x,
				this.stageData[key]["Position"]["Y"].Value<float>() + basePos.y,
				this.stageData[key]["Position"]["Z"].Value<float>()
				);
		}

		obj.transform.eulerAngles = new Vector3(
			this.stageData[key]["Rotation"]["X"].Value<float>(),
			this.stageData[key]["Rotation"]["Y"].Value<float>(),
			this.stageData[key]["Rotation"]["Z"].Value<float>()
			);

        obj.transform.localScale = new Vector3(
			this.stageData[key]["Scale"]["X"].Value<float>(),
			this.stageData[key]["Scale"]["Y"].Value<float>(),
			this.stageData[key]["Scale"]["Z"].Value<float>()
			);

        createdObjs.Add(GetBlockObjectID(int.Parse(key)), obj);

        return obj;
    }

    private GameObject CreateEmptyObject(string key, Vector3 basePos) {
		GameObject obj = new GameObject();
        obj.transform.position = new Vector3(
			this.stageData[key]["Position"]["X"].Value<float>() + basePos.x,
			this.stageData[key]["Position"]["Y"].Value<float>() + basePos.y,
			this.stageData[key]["Position"]["Z"].Value<float>()
			);
        obj.transform.eulerAngles = new Vector3(
			this.stageData[key]["Rotation"]["X"].Value<float>(),
			this.stageData[key]["Rotation"]["Y"].Value<float>(),
			this.stageData[key]["Rotation"]["Z"].Value<float>()
			);
        obj.transform.localScale = new Vector3(
			this.stageData[key]["Scale"]["X"].Value<float>(),
			this.stageData[key]["Scale"]["Y"].Value<float>(),
			this.stageData[key]["Scale"]["Z"].Value<float>()
			);

		this.createdObjs.Add(GetBlockObjectID(int.Parse(key)), obj);
        return obj;
    }

    private void ApplyComponents(GameObject obj, string key) {
        if (this.stageData[key]["Name"])
            obj.name = this.stageData[key]["Name"].Value<string>();

        if (this.stageData[key]["Tag"].Value<string>() == "WorldText")
            obj.GetComponentInChildren<Text>().text = this.stageData[key]["Text"].Value<string>();

        if (this.stageData[key]["Layer"])
            obj.layer = this.stageData[key]["Layer"].Value<int>();

        if (this.stageData[key]["Sprite"])
			this.ApplySpriteRenderer(obj, key);

        if (this.stageData[key]["Controller"])
			this.ApplyAnimator(obj, key);

        if (this.stageData[key]["Spawner"])
			this.ApplySpawner(obj, key);

        if (this.stageData[key]["Trigger"])
			this.ApplyTrigger(obj, key);

        if (this.stageData[key]["BoxCollider"])
			this.ApplyBoxCollider(obj, key);

        if (this.stageData[key]["Light"])
			this.ApplyLight(obj, key);

        if (this.stageData[key]["Parent"]) {
            if(this.stageData[key]["ParentIsCam"]) {
				this.inheritances.Add(
					GetBlockObjectID(int.Parse(key)),
					Camera.main.GetInstanceID()
					);
            }
            else {
				this.inheritances.Add(
                    GetBlockObjectID(int.Parse(key)),
                    GetParentBlockObjectID(this.stageData[key]["Parent"].Value<int>())
					);
            }
        }
    }

    private void ApplySpriteRenderer(GameObject obj, string key) {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (!sr)
			sr = obj.AddComponent<SpriteRenderer>();

		JDictionary spriteData = this.stageData[key]["Sprite"];

		sr.sprite = Helper.GetSprite(
			spriteData["SpritePath"].Value<string>(),
			spriteData["SpriteName"].Value<string>()
			);

		sr.material = Helper.GetMaterial(
			spriteData["MaterialPath"].Value<string>(),
			spriteData["MaterialName"].Value<string>()
			);

        sr.flipX = (spriteData["FlipX"].Value<int>() == 1);
        sr.flipY = (spriteData["FlipY"].Value<int>() == 1);

        sr.sortingLayerName = spriteData["SortingLayer"].Value<string>();
        sr.sortingOrder		= spriteData["SortOrder"].Value<int>();
    }

    private void ApplyAnimator(GameObject obj, string key) {
        Animator anim = obj.GetComponent<Animator>();
        if (!anim)
            anim = obj.AddComponent<Animator>();

		JDictionary animCtrlData = this.stageData[key]["Controller"];

		anim.runtimeAnimatorController = (RuntimeAnimatorController) Resources.Load(animCtrlData.Value<string>());
    }

    private void ApplySpawner(GameObject obj, string key) {
        CharacterSpawner cs = obj.GetComponent<CharacterSpawner>();
        if (!cs)
            cs = obj.AddComponent<CharacterSpawner>();

		JDictionary spawnerData = this.stageData[key]["Spawner"];

		cs.delay			= spawnerData["Delay"].Value<float>();
        cs.characterClass	= spawnerData["CharacterClass"].Value<string>();
		this.PreloadSprites("characters", cs.characterClass);
		cs.team				= (Teams) spawnerData["Team"].Value<int>();
        cs.weaponClass		= spawnerData["WeaponClass"].Value<string>();
		cs.characterType	= (CharacterTypes) spawnerData["CharacterType"].Value<int>();
		cs.spawnerType		= (CharacterSpawnerTypes) spawnerData["SpawnerType"].Value<int>();
        cs.facingRight      = spawnerData["FacingRight"].Value<bool>();
        cs.autoEngage       = spawnerData["AutoEngage"].Value<bool>();
        cs.startEnabled     = spawnerData["StartEnabled"].Value<bool>();
    }

    private void ApplyTrigger(GameObject obj, string key) {
        Trigger tr = obj.GetComponent<Trigger>();
        if (!tr)
            tr = obj.AddComponent<Trigger>();

		JDictionary triggerData = this.stageData[key]["Trigger"];

		tr.action = triggerData["Action"].Value<string>();

        List<string> args = new List<string>();
        foreach (JDictionary arg in triggerData["Arguments"]) {
            args.Add(arg.Value<string>());
        }
        tr.arguments = args.ToArray();

        List<GameObject> targets = new List<GameObject>();
        foreach (JDictionary target in triggerData["Targets"]) {
            targets.Add(GetBlockObjectByID(int.Parse(target.Value<string>())));
        }
        tr.targets = targets.ToArray();

        tr.Initialize();
    }

    private void ApplyBoxCollider(GameObject obj, string key) {
        BoxCollider2D box = obj.GetComponent<BoxCollider2D>();
        if (!box)
            box = obj.AddComponent<BoxCollider2D>();

		JDictionary colliderData = this.stageData[key]["BoxCollider"];

		box.offset = new Vector2(
			colliderData["BoxOffset"]["X"].Value<float>(), 
			colliderData["BoxOffset"]["Y"].Value<float>()
			);
        box.size = new Vector2(
			colliderData["BoxSize"]["X"].Value<float>(), 
			colliderData["BoxSize"]["Y"].Value<float>()
			);
    }

    private void ApplyLight(GameObject obj, string key) {
        Light light = obj.GetComponent<Light>();
        if (!light)
            light = obj.AddComponent<Light>();

		JDictionary lightData = this.stageData[key]["Light"];

		light.type		= (LightType) lightData["Type"].Value<int>();
        light.range		= lightData["Range"].Value<float>();
        light.intensity = lightData["Intensity"].Value<float>();
        light.color		= new Color(
			lightData["Color"]["R"].Value<float>(), 
			lightData["Color"]["G"].Value<float>(), 
			lightData["Color"]["B"].Value<float>()
			);

        if(lightData["CookiePath"])
            light.cookie = Helper.GetTexture(
				lightData["CookiePath"].Value<string>(), 
				lightData["CookieName"].Value<string>()
				);

        if(lightData["FlarePath"])
            light.flare = Helper.GetFlare(
				lightData["FlarePath"].Value<string>(),
                lightData["FlareName"].Value<string>()
				);

        light.renderMode = (LightRenderMode) lightData["RenderMode"].Value<int>();
        light.cullingMask = lightData["CullingMask"].Value<int>();
    }

    public string CurrentMap() {
        return currentMap;
    }

    public void DestroyMap() {
		isMapActive = false;
        EventManager.OnMapDestroyed();
        EventManager.UnregisterAll();
        if(createdObjs.Count > 0) {
            foreach(KeyValuePair<int, GameObject> kvp in createdObjs) {
				if(kvp.Value != null) {
					Destroy(kvp.Value);
				}
			}
        }
		createdObjs.Clear();
        List<Character> characters = CharacterManager.GetAllCharacters();
        for (int i = characters.Count - 1; i > -1; i--) {
            characters[i].DestroyCharacter(false);
        }
        List<Effect> effects = EffectManager.GetEffects();
        for (int i = 0; i < effects.Count; i++) {
            EffectManager.RemoveEffect(effects[i]);
        }
        List<Loot> loots = LootManager.GetLoots();
        for (int i = 0; i < loots.Count; i++) {
            LootManager.RemoveLoot(loots[i]);
        }
        currentMap = "";
        lastBlockID = 0;
    }

    public void Initialize() {
        if (!isMapActive)
            Cursor.visible = true;
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
            LoadingScreen.instance.Close();
    }

    public Vector2 GetMapMin() {
        return mapMin;
    }

    public Vector2 GetMapMax() {
        return mapMax;
    }


}
