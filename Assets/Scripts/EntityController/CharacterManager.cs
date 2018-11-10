using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* 캐릭터 생성, 관리 클래스
 * 캐릭터의 생성과 적/아군 가져오기, 범위 내 캐릭터 가져오기 등의 기능 제공 */
public static class CharacterManager {
    private static List<Character> Characters = new List<Character>();
    private static Character player;

    private static EntityManager em;
    public static void SetEntityManager(EntityManager _em) {
        em = _em;
    }

    public static Character CreateCharacter(string classname, Vector3 pos, Teams team, int sortorder = 0) {
		if(!LevelManager.instance.isMapActive)
			return null;

        GameObject character_obj = em.GetCharacterFromPool();
        character_obj.transform.position = pos;

        string scriptClass = GameDataManager.instance.RootData[classname]["ScriptClass"].Value<string>();
		Character character = (Character)character_obj.AddComponent(Type.GetType(scriptClass));
        character.Initialize(classname);
        character.SetTeam(team);
        character_obj.GetComponentInChildren<SpriteRenderer>().sortingOrder = sortorder;
        Characters.Add(character);
        InventoryManager.CreateInventory(character_obj);

		EventManager.OnCharacterCreated(character, pos);

        return character;
    }

    public static void InsertAI(Character ai, string aidata, bool isBoss) {
        string scriptClass = GameDataManager.instance.RootData[aidata]["ScriptClass"].Value<string>();

		if (!isBoss) {
            AIBaseController aicon;
            if (scriptClass != "") {
				aicon = (AIBaseController) ai.gameObject.AddComponent(Type.GetType(scriptClass));
            }
            else {
                aicon = ai.gameObject.AddComponent<AIBaseController>();
            }
            aicon.Initialize(ai, aidata);
            ai.SetController(aicon);
        }
        else {
            AIBossController aicon;
            if (scriptClass != "") {
				aicon = (AIBossController) ai.gameObject.AddComponent(Type.GetType(scriptClass));
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

    public static void InsertControl(Character c) {
        c.gameObject.AddComponent<GeneralControl>();
        c.gameObject.GetComponent<GeneralControl>().Initialize(c);
        c.SetController(c.gameObject.GetComponent<GeneralControl>());
        player = c;
    }

    public static void OnCharacterDeath(Character c) {
        Characters.Remove(c);
        em.AddCharacterToPool(c.gameObject);
        UnityEngine.Object.Destroy(c);

		EventManager.OnCharacterDeath(c);
    }

    public static Character GetPlayer() {
        return player;
    }

    public static List<Character> GetAllCharacters() {
        return Characters;
    }

    public static List<Character> GetAllies(Teams team) {
        return Characters.Where(c => c.GetTeam() == team).ToList();
    }

    public static List<Character> GetEnemies(Teams team) {
        return Characters.Where(c => c.GetTeam() != team).ToList();
    }

    public static Character GetClosestAlly(Vector3 pos, Teams team) {
        return Characters.Where(c => c.GetTeam() == team).OrderBy(c => Vector3.Distance(Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), pos), pos)).FirstOrDefault();
    }

    public static Character GetClosestEnemy(Vector3 pos, Teams team) {
        return Characters.Where(c => c.GetTeam() != team).OrderBy(c => Vector3.Distance(Helper.GetClosestBoxBorder(c.transform.position, c.GetComponent<BoxCollider2D>(), pos), pos)).FirstOrDefault();
    }
}
