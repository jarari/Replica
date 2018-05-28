using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        GameObject character_obj = (GameObject)Instantiate(Resources.Load("Prefab/Character"), pos, new Quaternion());
        Character character = (Character)character_obj.AddComponent(Type.GetType((string)GameDataManager.instance.GetData("Data", classname, "ScriptClass")));
        character.Initialize(classname);
        character.SetTeam(team);
        character_obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = sortorder;
        Characters.Add(character);
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
        return Characters.Where(c => c.GetTeam() == team).OrderBy(c => Vector3.Distance(c.transform.position, pos)).FirstOrDefault();
    }

    public Character GetClosestEnemy(Vector3 pos, Teams team) {
        return Characters.Where(c => c.GetTeam() != team).OrderBy(c => Vector3.Distance(c.transform.position, pos)).FirstOrDefault();
    }
}
