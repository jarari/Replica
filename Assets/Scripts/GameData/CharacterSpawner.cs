using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CharacterSpawnerTypes {
    Continuous,
    OnDeath,
    Once
}
public class CharacterSpawner : MonoBehaviour {
    private List<Character> managed = new List<Character>();
    private Character lastspawn = null;
    public float delay = -1;
    private float nextSpawn;
    private bool nextSpawnReseted = true;
    public string characterClass;
    public Teams team;
    public string weaponClass;
    public CharacterTypes characterType;
    public CharacterSpawnerTypes spawnerType = CharacterSpawnerTypes.Once;
    private string aiScript = "";
    public Character target = null;
    public bool autoEngage = false;
    public bool init = false;
    public void Initialize() {
        Initialize(characterClass, team, weaponClass, characterType, spawnerType, target);
    }
    public void Initialize(string c, Teams t, string w, CharacterTypes tp, CharacterSpawnerTypes sptp, Character forcetarget = null) {
        characterClass = c;
        team = t;
        weaponClass = w;
        characterType = tp;
        spawnerType = sptp;
        target = forcetarget;
        nextSpawn = Time.realtimeSinceStartup;
        aiScript = (string)GameDataManager.instance.GetData("Data", c, "ScriptClass");
        init = true;
        if (spawnerType == CharacterSpawnerTypes.Once)
            Spawn();
    }

    private void Spawn() {
        int sortorder = 0;
        if (characterType == CharacterTypes.Player)
            sortorder = 5;
        Character spawned = CharacterManager.instance.CreateCharacter(characterClass, transform.position, team, sortorder);
        spawned.GiveWeapon(weaponClass);
        switch (characterType) {
            case CharacterTypes.Player:
                CharacterManager.instance.InsertControl(spawned);
                CamController.instance.AttachCam(spawned.transform);
                //Save.DataLoad();
                //PlayerHUD.Initialize();
                //PlayerHUD.UpdateHealth(spawned.GetCurrentStat(CharacterStats.Health) / spawned.GetMaxStat(CharacterStats.Health));
                break;
            case CharacterTypes.AI:
                if(aiScript.Length != 0) {
                    CharacterManager.instance.InsertAI(spawned, aiScript, false);
                    spawned.GetComponent<AIBaseController>().ForceTarget(target);
                }
                break;
            case CharacterTypes.Boss:
                if (aiScript.Length != 0) {
                    CharacterManager.instance.InsertAI(spawned, aiScript, true);
                }
                break;
        }
        managed.Add(spawned);
        lastspawn = spawned;
        nextSpawn = Time.realtimeSinceStartup + delay;
        nextSpawnReseted = false;
        if (spawnerType == CharacterSpawnerTypes.Once)
            gameObject.SetActive(false);
    }

    public void SetDelay(float d) {
        delay = d;
    }

    public void SetAutoEngage(bool b) {
        autoEngage = b;
    }

    private void Update() {
        if (!init) return;
        if (autoEngage) {
            Character closest = CharacterManager.instance.GetClosestEnemy(transform.position, team);
            if (closest != null)
                target = closest;
        }
        if (spawnerType == CharacterSpawnerTypes.Continuous && nextSpawn <= Time.realtimeSinceStartup && delay != -1)
            Spawn();
        else if (spawnerType == CharacterSpawnerTypes.OnDeath && lastspawn == null) {
            if (delay == -1)
                Spawn();
            else if (!nextSpawnReseted) {
                nextSpawnReseted = true;
                nextSpawn = Time.realtimeSinceStartup + delay;
            }
            else if (nextSpawn <= Time.realtimeSinceStartup)
                Spawn();
        }
    }

    private void OnDestroy() {
        foreach(Character c in managed) {
            if(c != null)
                DestroyObject(c.transform.gameObject);
        }
    }
}
