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

	private JDictionary                     rootData;
	private Dictionary<string, ComboData>	basicCombos;
    private Dictionary<string, ComboData>	continuousCombos;

	public JDictionary						RootData {
		get {
			return this.rootData;
		}
	}
	public Dictionary<string, ComboData>	BasicComboData {
		get {
			return basicCombos;
		}
	}
	public Dictionary<string, ComboData>	ContinuousComboData {
		get {
			return continuousCombos;
		}
	}

	void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);

		if(LoadingScreen.instance != null)
            LoadingScreen.instance.Close();

		rootData = new JDictionary();
		rootData.DeserializeJson(
			"Data/characters/main",
			"Data/weapons/weapondata",
			"Data/effects/effectdata",
			"Data/effects/particledata",
			"Data/ai/aidata",
			"Data/items/itemdata",
			"Data/items/lootdata",
			"Data/maps/mapdata",
			"Data/weapons/attackdata",
			"Data/weapons/bulletdata",
			"Data/weapons/projectiledata"
			);

		ParseComboData();

		if (!debug)
            GlobalUIManager.instance.LoadScene("1");
    }

	private void ParseComboData() {
		basicCombos = new Dictionary<string, ComboData>();
		continuousCombos = new Dictionary<string, ComboData>();

		JDictionary tempData = new JDictionary();
		tempData.DeserializeJson("Data/weapons/attackdata");

		foreach(JDictionary subData in tempData) {
			JDictionary comboData = subData["Combo"];

			if(comboData) {
				List<KeyCombo> keyCombos = new List<KeyCombo>();
				List<string> nextPossibleCombos = new List<string>();

				bool isBasic = (comboData["IsBasic"] ? comboData["IsBasic"].Value<bool>() : false);
				bool isJumpAttack = (comboData["IsJumpAttack"] ? comboData["IsJumpAttack"].Value<bool>() : false);

				foreach(JDictionary keycombo in comboData["Keys"]) {
					switch(keycombo.Value<string>()) {
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

				if(comboData["PossibleCombos"]) {
					foreach(JDictionary nextcombo in comboData["PossibleCombos"]) {
						nextPossibleCombos.Add(nextcombo.Value<string>());
					}
				}

				if(isBasic) {
					basicCombos.Add(subData.Key, new ComboData(keyCombos, nextPossibleCombos, isJumpAttack));
					continuousCombos.Add(subData.Key, new ComboData(keyCombos, nextPossibleCombos, isJumpAttack));
				}
				else {
					continuousCombos.Add(subData.Key, new ComboData(keyCombos, nextPossibleCombos, isJumpAttack));
				}
			}
		}
	}

	/* 빠른 접근을 위한 스탯 관련 함수들 */
	public float GetCharacterStat(string classname, CharacterStats stat) {
		JDictionary statData = RootData[classname]["Stats"];

        switch (stat) {
            case CharacterStats.Health:
				return (statData["Health"] ? statData["Health"].Value<float>() : -1.0f);

			case CharacterStats.MoveSpeed:
				return (statData["MoveSpeed"] ? statData["MoveSpeed"].Value<float>() : -1.0f);

			case CharacterStats.JumpPower:
				return (statData["JumpPower"] ? statData["JumpPower"].Value<float>() : -1.0f);

			case CharacterStats.MeleeArmor:
				return (statData["MeleeArmor"] ? statData["MeleeArmor"].Value<float>() : -1.0f);

			case CharacterStats.RangeArmor:
				return (statData["RangeArmor"] ? statData["RangeArmor"].Value<float>() : -1.0f);

			case CharacterStats.GrenadeFullCharge:
				return (statData["GrenadeFullCharge"] ? statData["GrenadeFullCharge"].Value<float>() : 2.0f);

			case CharacterStats.GrenadeThrowPower:
				return (statData["GrenadeThrowPower"] ? statData["GrenadeThrowPower"].Value<float>() : 600.0f);

			default:
				return -1;
        }
    }

    public float GetWeaponStat(string classname, WeaponStats stat) {
		JDictionary statData = RootData[classname]["Stats"];

        switch (stat) {
            case WeaponStats.AttackSpeed:
				return (statData["AttackSpeed"]	? statData["AttackSpeed"].Value<float>() : -1.0f);

            case WeaponStats.Damage:
				return (statData["Damage"] ? statData["Damage"].Value<float>() : -1.0f);

			case WeaponStats.MagSize:
				return (statData["MagSize"]	? statData["MagSize"].Value<float>() : -1.0f);

			case WeaponStats.ReloadTime:
				return (statData["ReloadTime"] ? statData["ReloadTime"].Value<float>() : -1.0f);

			case WeaponStats.SADestruction:
				return (statData["SADestruction"] ? statData["SADestruction"].Value<float>() : -1.0f);

			case WeaponStats.Spread:
				return (statData["Spread"] ? statData["Spread"].Value<float>() : -1.0f);

			case WeaponStats.Stagger:
				return (statData["Stagger"] ? statData["Stagger"].Value<float>() : -1.0f);

			case WeaponStats.Range:
				return (statData["Range"] ? statData["Range"].Value<float>() : -1.0f);

			case WeaponStats.BulletSpeed:
				return (statData["BulletSpeed"] ? statData["BulletSpeed"].Value<float>() : -1.0f);

			case WeaponStats.ExplosionRadius:
				return (statData["ExplosionRadius"] ? statData["ExplosionRadius"].Value<float>() : -1.0f);

			default:
				return -1.0f;
        }
    }

	/// <summary>
	/// 애니메이터 컨트롤러를 가져오는 함수. JSON 데이터에 컨트롤러가 여러개 적혀있는 경우가 있는데, 그 컨트롤러 중 하나를 랜덤하게 가져옴. 주로 총알 피격이펙트 랜덤 재생에 사용됨.
	/// </summary>
	/// <param name="classname"></param>
	/// <returns></returns>
	public RuntimeAnimatorController GetAnimatorController(string classname) {
        JDictionary controllerData = rootData[classname]["Sprites"]["controller"];

		string controllername;
		if(controllerData.IsValue) {
			controllername = controllerData.Value<string>();
		}
		else {
			int rand = UnityEngine.Random.Range(0, controllerData.Count);
			controllername = controllerData[rand.ToString()].Value<string>();
		}

        var controller = Resources.Load("AnimatorController/" + controllername);

		if (!controller) return null;

		if(controller.GetType().Equals(typeof(AnimatorOverrideController)))
			return (AnimatorOverrideController) controller;

		return (RuntimeAnimatorController) controller;
    }
}
