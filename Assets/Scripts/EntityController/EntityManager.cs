using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//오브젝트 풀링 관리 매니저
//이전에 맵에 존재하던 아래 클래스들은 MonoBehaviour가 아닌 정적 C# 클래스로 처리
//파티클은 파티클이 개별 프리팹이기 때문에 풀링 불가. 독립 매니저로 관리
//아직 게임에 사운드가 없으므로 사운드 매니저는 추후 수정.
public class EntityManager : MonoBehaviour {
    private int max_bullets = 1000;
    private int max_projectiles = 1000;
    private int max_lasers = 500;
    private int max_characters = 100;
    private int max_effects = 10000;
    private int max_loots = 100;

	private GameObject pool_bullets_parent;
	private GameObject pool_projectiles_parent;
	private GameObject pool_lasers_parent;
	private GameObject pool_characters_parent;
	private GameObject pool_effects_parent;
	private GameObject pool_loots_parent;

	private GameObject pool_bullets_prefab;
	private GameObject pool_projectiles_prefab;
	private GameObject pool_lasers_prefab;
	private GameObject pool_characters_prefab;
	private GameObject pool_effects_prefab;
	private GameObject pool_loots_prefab;

    private Stack<GameObject> pool_bullets;
    private Stack<GameObject> pool_projectiles;
    private Stack<GameObject> pool_lasers;
    private Stack<GameObject> pool_characters;
    private Stack<GameObject> pool_effects;
    private Stack<GameObject> pool_loots;

    void Awake() {
        //매니저 생성시 해당 엔티티매니저를 기록
        BulletManager.SetEntityManager(this);
        CharacterManager.SetEntityManager(this);
        EffectManager.SetEntityManager(this);
        LootManager.SetEntityManager(this);

        //풀 스택 생성
        InitializePools();

        //풀 채우기
        FillPoolsWithObjects();
    }

    private void InitializePools() {
		this.pool_bullets_parent		= new GameObject("Pool_Bullets");
		this.pool_projectiles_parent	= new GameObject("Pool_Projectiles");
		this.pool_lasers_parent			= new GameObject("Pool_Lasers");
		this.pool_characters_parent		= new GameObject("Pool_Characters");
		this.pool_effects_parent		= new GameObject("Pool_Effects");
		this.pool_loots_parent			= new GameObject("Pool_Loots");

		this.pool_bullets		= new Stack<GameObject>(this.max_bullets);
		this.pool_projectiles	= new Stack<GameObject>(this.max_projectiles);
		this.pool_lasers		= new Stack<GameObject>(this.max_lasers);
		this.pool_characters	= new Stack<GameObject>(this.max_characters);
		this.pool_effects		= new Stack<GameObject>(this.max_effects);
		this.pool_loots			= new Stack<GameObject>(this.max_loots);

		this.pool_bullets_prefab		= Instantiate(Resources.Load("Prefab/Bullet")) as GameObject;
		this.pool_projectiles_prefab	= Instantiate(Resources.Load("Prefab/Projectile")) as GameObject;
		this.pool_lasers_prefab			= Instantiate(Resources.Load("Prefab/Laser")) as GameObject;
		this.pool_characters_prefab		= Instantiate(Resources.Load("Prefab/Character")) as GameObject;
		this.pool_effects_prefab		= Instantiate(Resources.Load("Prefab/Effect")) as GameObject;
		this.pool_loots_prefab			= Instantiate(Resources.Load("Prefab/Loot")) as GameObject;

		this.pool_bullets_prefab.name		= "Pool_Bullets_Prefab";
		this.pool_projectiles_prefab.name	= "Pool_Projectiles_Prefab";
		this.pool_lasers_prefab.name		= "Pool_Lasers_Prefab";
		this.pool_characters_prefab.name	= "Pool_Characters_Prefab";
		this.pool_effects_prefab.name		= "Pool_Effects_Prefab";
		this.pool_loots_prefab.name			= "Pool_Loots_Prefab";

		this.pool_bullets_prefab.transform.SetParent(this.pool_bullets_parent.transform);
		this.pool_projectiles_prefab.transform.SetParent(this.pool_projectiles_parent.transform);
		this.pool_lasers_prefab.transform.SetParent(this.pool_lasers_parent.transform);
		this.pool_characters_prefab.transform.SetParent(this.pool_characters_parent.transform);
		this.pool_effects_prefab.transform.SetParent(this.pool_effects_parent.transform);
		this.pool_loots_prefab.transform.SetParent(this.pool_loots_parent.transform);
    }

    private void FillPoolsWithObjects() {
        for(int i = 0; i < this.max_bullets; i++) {
			GameObject obj = Instantiate(this.pool_bullets_prefab, Vector3.zero, Quaternion.identity);
			obj.name = "Bullet";

			this.PushBulletToPool(obj);
        }
		for (int i = 0; i < this.max_projectiles; i++) {
			GameObject obj = Instantiate(this.pool_projectiles_prefab, Vector3.zero, Quaternion.identity);
			obj.name = "Projectile";

			this.PushProjectileToPool(obj);
        }
		for (int i = 0; i < this.max_lasers; i++) {
			GameObject obj = Instantiate(this.pool_lasers_prefab, Vector3.zero, Quaternion.identity);
			obj.name = "Laser";

			this.PushLaserToPool(obj);
        }
		for (int i = 0; i < this.max_characters; i++) {
			GameObject obj = Instantiate(this.pool_characters_prefab, Vector3.zero, Quaternion.identity);
			obj.name = "Character";

			this.PushCharacterToPool(obj);
        }
		for (int i = 0; i < this.max_effects; i++) {
			GameObject obj = Instantiate(this.pool_effects_prefab, Vector3.zero, Quaternion.identity);
			obj.name = "Effect";

			this.PushEffectToPool(obj);
        }
		for (int i = 0; i < this.max_loots; i++) {
			GameObject obj = Instantiate(this.pool_loots_prefab, Vector3.zero, Quaternion.identity);
			obj.name = "Loot";

			this.PushLootToPool(obj);
        }
    }

	public static bool IsManagedObject(GameObject obj) {
		return obj.GetComponent<Bullet>()
			|| obj.GetComponent<Projectile>()
			|| obj.GetComponent<Laser>()
			|| obj.GetComponent<Character>()
			|| obj.GetComponent<Effect>()
			|| obj.GetComponent<Loot>();
	}

	public GameObject PullBulletFromPool() {
        if(pool_bullets.Count > 0) {
            GameObject obj = pool_bullets.Pop();

            obj.SetActive(true);
            return obj;
        }

        Debug.Log("Cannot create more bullets.");

        return null;
    }
    public GameObject PullProjectileFromPool() {
        if (pool_projectiles.Count > 0) {
            GameObject obj = pool_bullets.Pop();
            obj.SetActive(true);
            return obj;
        }

        Debug.Log("Cannot create more projectiles.");

        return null;
    }
    public GameObject PullLaserFromPool() {
        if (pool_lasers.Count > 0) {
            GameObject obj = pool_lasers.Pop();
            obj.SetActive(true);

            return obj;
        }

        Debug.Log("Cannot create more lasers.");

        return null;
    }
    public GameObject PullCharacterFromPool() {
        if (pool_characters.Count > 0) {
            GameObject obj = pool_characters.Pop();
            obj.SetActive(true);

            return obj;
        }

        Debug.Log("Cannot create more characters.");

        return null;
    }
    public GameObject PullEffectFromPool() {
        if (pool_effects.Count > 0) {
            GameObject obj = pool_effects.Pop();
            obj.SetActive(true);

            return obj;
        }

        Debug.Log("Cannot create more effects.");

        return null;
    }
    public GameObject PullLootFromPool() {
        if (pool_loots.Count > 0) {
            GameObject obj = pool_loots.Pop();
            obj.SetActive(true);

            return obj;
        }

        Debug.Log("Cannot create more loots.");

        return null;
	}

	// asdf 
	public void PushToPool(GameObject obj) {

	}

	public void PushBulletToPool(GameObject obj) {
		obj.SetActive(false);
		obj.transform.SetParent(pool_bullets_parent.transform);

		pool_bullets.Push(obj);
	}

	public void PushProjectileToPool(GameObject obj) {
		obj.SetActive(false);
		obj.transform.SetParent(pool_projectiles_parent.transform);
		pool_projectiles.Push(obj);
	}

	public void PushLaserToPool(GameObject obj) {
		obj.SetActive(false);
		obj.transform.SetParent(pool_lasers_parent.transform);
		pool_lasers.Push(obj);
	}

	public void PushCharacterToPool(GameObject obj) {
		obj.SetActive(false);
		obj.transform.SetParent(pool_characters_parent.transform);
		pool_characters.Push(obj);
	}

	public void PushEffectToPool(GameObject obj) {
		obj.SetActive(false);
		obj.transform.SetParent(pool_effects_parent.transform);
		pool_effects.Push(obj);
	}

	public void PushLootToPool(GameObject obj) {
        obj.SetActive(false);
		obj.transform.SetParent(pool_loots_parent.transform);
		pool_loots.Push(obj);
    }

}
