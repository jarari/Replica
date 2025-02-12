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
    private float nextSpawn;
    private bool nextSpawnReseted = true;
    public float delay = -1;
    public string characterClass;
    public Teams team;
    public string weaponClass;
    public CharacterTypes characterType;
    public CharacterSpawnerTypes spawnerType = CharacterSpawnerTypes.Once;
    private string aiScript = "";
    public Character target = null;
    public bool facingRight = true;
    public bool autoEngage = false;
    public bool startEnabled = false;
    public bool init = false;

	private JDictionary characterData;

    public void Initialize() {
        Initialize(characterClass, team, weaponClass, characterType, spawnerType, target);
    }
    public void Initialize(string c, Teams t, string w, CharacterTypes tp, CharacterSpawnerTypes sptp, Character forcetarget = null) {
        characterClass = c;
		characterData = GameDataManager.instance.RootData[characterClass];

        team = t;
        weaponClass = w;
        characterType = tp;
        spawnerType = sptp;
        target = forcetarget;
        nextSpawn = Time.realtimeSinceStartup;
        if(characterData["AIScript"])
            aiScript = characterData["AIScript"].Value<string>();
        if (startEnabled) {
            init = true;
            if (spawnerType == CharacterSpawnerTypes.Once || spawnerType == CharacterSpawnerTypes.OnDeath)
                Spawn();
        }
    }

    public void Spawn() {
        int sortorder = 0;
        if (characterType == CharacterTypes.Player)
            sortorder = 5;
        Character spawned = CharacterManager.CreateCharacter(characterClass, transform.position, team, sortorder);
        if (spawned == null)
            return;
        switch (characterType) {
            case CharacterTypes.Player:
                spawned.GiveWeapon("weapon_fist");
                spawned.GiveWeapon("weapon_gunkata");
                spawned.GiveWeapon("weapon_sword");
                CharacterManager.InsertControl(spawned);
                PlayerHUD.Initialize();
                CamController.instance.AttachCam(spawned.transform);
                Save.LoadData();
                PlayerHUD.UpdateHealth(spawned, spawned.GetCurrentStat(CharacterStats.Health) / spawned.GetMaxStat(CharacterStats.Health));
                break;
            case CharacterTypes.AI:
                spawned.GiveWeapon(weaponClass);
                if (aiScript.Length != 0) {
                    CharacterManager.InsertAI(spawned, aiScript, false);
                    spawned.GetComponent<AIBaseController>().ForceTarget(target);
                }
                break;
            case CharacterTypes.Boss:
                spawned.GiveWeapon(weaponClass);
                spawned.SetFlag(CharacterFlags.KnockBackImmunity);
                spawned.SetFlag(CharacterFlags.StaggerImmunity);
                if (aiScript.Length != 0) {
                    CharacterManager.InsertAI(spawned, aiScript, true);
                }
                break;
        }
        spawned.FlipFace(facingRight);
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

    public Character GetLastSpawn() {
        return lastspawn;
    }

    private void Update() {
        if (!init || !LevelManager.instance.isMapActive) return;
        if (autoEngage) {
            Character closest = CharacterManager.GetClosestEnemy(transform.position, team);
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
}
