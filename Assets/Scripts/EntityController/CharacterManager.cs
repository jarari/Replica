using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* 캐릭터 생성, 관리 클래스
 * 캐릭터의 생성과 적/아군 가져오기, 범위 내 캐릭터 가져오기 등의 기능 제공 */
public class CharacterManager : MonoBehaviour {
    private List<Character> Characters = new List<Character>();
    private Character player;
    public static CharacterManager instance;
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
        /*foreach(GameObject spawner in GameObject.FindGameObjectsWithTag("Spawner")) {
            spawner.GetComponent<CharacterSpawner>().Initialize();
        }*/
    }

    public Character CreateCharacter(string classname, Vector3 pos, Teams team, int sortorder = 0) {
		if(!LevelManager.instance.isMapActive) return null;
		GameObject character_obj = (GameObject)Instantiate(Resources.Load("Prefab/Character"), pos, new Quaternion());
        Character character = (Character)character_obj.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", classname, "ScriptClass")));
        character.Initialize(classname);
        character.SetTeam(team);
        character_obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = sortorder;
        Characters.Add(character);
        InventoryManager.CreateInventory(character_obj);
        if(EventManager.Event_CharacterCreated != null)
            EventManager.Event_CharacterCreated(character, pos);
        return character;
    }

    public void InsertAI(Character ai, string aidata, bool isBoss) {
        if (!isBoss) {
            AIBaseController aicon;
            if ((string)GameDataManager.instance.GetData("Data", aidata, "ScriptClass") != "") {
                aicon = (AIBaseController)ai.gameObject.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", aidata, "ScriptClass")));
            }
            else {
                aicon = ai.gameObject.AddComponent<AIBaseController>();
            }
            aicon.Initialize(ai, aidata);
            ai.SetController(aicon);
        }
        else {
            AIBossController aicon;
            if ((string)GameDataManager.instance.GetData("Data", aidata, "ScriptClass") != "") {
                aicon = (AIBossController)ai.gameObject.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", aidata, "ScriptClass")));
            }
            else {
                aicon = ai.gameObject.AddComponent<AIBossController>();
            }
            aicon.Initialize(ai, aidata);
            ai.SetController(aicon);
            ai.SetFlag(CharacterFlags.Boss);
        }
        ai.SetFlag(CharacterFlags.AIControlled);
    }

    public void InsertControl(Character c) {
        c.gameObject.AddComponent<GeneralControl>();
        c.gameObject.GetComponent<GeneralControl>().Initialize(c);
        c.SetController(c.gameObject.GetComponent<GeneralControl>());
        player = c;
    }

    public void OnCharacterDead(Character c) {
        Characters.Remove(c);
        if (EventManager.Event_CharacterDeath != null)
            EventManager.Event_CharacterDeath(c);
    }

    public Character GetPlayer() {
        return player;
    }

    public List<Character> GetAllCharacters() {
        return Characters;
    }

    public List<Character> GetAllies(Teams team) {
        return Characters.Where(c => c.GetTeam() == team).ToList();
    }

    public List<Character> GetEnemies(Teams team) {
        return Characters.Where(c => c.GetTeam() != team).ToList();
    }

    public Character GetClosestAlly(Vector3 pos, Teams team) {
        return Characters.Where(c => c.GetTeam() == team).OrderBy(c => Vector3.Distance(Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), pos), pos)).FirstOrDefault();
    }

    public Character GetClosestEnemy(Vector3 pos, Teams team) {
        return Characters.Where(c => c.GetTeam() != team).OrderBy(c => Vector3.Distance(Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), pos), pos)).FirstOrDefault();
    }
}
