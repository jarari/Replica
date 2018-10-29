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
        pool_bullets = new Stack<GameObject>(max_bullets);
        pool_projectiles = new Stack<GameObject>(max_projectiles);
        pool_lasers = new Stack<GameObject>(max_lasers);
        pool_characters = new Stack<GameObject>(max_characters);
        pool_effects = new Stack<GameObject>(max_effects);
        pool_loots = new Stack<GameObject>(max_loots);
    }

    private void FillPoolsWithObjects() {
        for(int i = 0; i < max_bullets; i++) {
            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Bullet"), new Vector3(), new Quaternion());
            AddBulletToPool(obj);
        }
        for (int i = 0; i < max_projectiles; i++) {
            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Projectile"), new Vector3(), new Quaternion());
            AddProjectileToPool(obj);
        }
        for (int i = 0; i < max_lasers; i++) {
            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Laser"), new Vector3(), new Quaternion());
            AddLaserToPool(obj);
        }
        for (int i = 0; i < max_characters; i++) {
            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Character"), new Vector3(), new Quaternion());
            AddCharacterToPool(obj);
        }
        for (int i = 0; i < max_effects; i++) {
            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Effect"), new Vector3(), new Quaternion());
            AddEffectToPool(obj);
        }
        for (int i = 0; i < max_loots; i++) {
            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Loot"), new Vector3(), new Quaternion());
            AddLootToPool(obj);
        }
    }

    public GameObject GetBulletFromPool() {
        if(pool_bullets.Count > 0) {
            GameObject obj = pool_bullets.Pop();
            obj.SetActive(true);
            return obj;
        }
        Debug.Log("Cannot create more bullets.");
        return null;
    }

    public void AddBulletToPool(GameObject obj) {
        obj.SetActive(false);
        pool_bullets.Push(obj);
    }

    public GameObject GetProjectileFromPool() {
        if (pool_projectiles.Count > 0) {
            GameObject obj = pool_bullets.Pop();
            obj.SetActive(true);
            return obj;
        }
        Debug.Log("Cannot create more projectiles.");
        return null;
    }

    public void AddProjectileToPool(GameObject obj) {
        obj.SetActive(false);
        pool_projectiles.Push(obj);
    }

    public GameObject GetLaserFromPool() {
        if (pool_lasers.Count > 0) {
            GameObject obj = pool_lasers.Pop();
            obj.SetActive(true);
            return obj;
        }
        Debug.Log("Cannot create more lasers.");
        return null;
    }

    public void AddLaserToPool(GameObject obj) {
        obj.SetActive(false);
        pool_lasers.Push(obj);
    }

    public GameObject GetCharacterFromPool() {
        if (pool_characters.Count > 0) {
            GameObject obj = pool_characters.Pop();
            obj.SetActive(true);
            return obj;
        }
        Debug.Log("Cannot create more characters.");
        return null;
    }

    public void AddCharacterToPool(GameObject obj) {
        obj.SetActive(false);
        pool_characters.Push(obj);
    }

    public GameObject GetEffectFromPool() {
        if (pool_effects.Count > 0) {
            GameObject obj = pool_effects.Pop();
            obj.SetActive(true);
            return obj;
        }
        Debug.Log("Cannot create more effects.");
        return null;
    }

    public void AddEffectToPool(GameObject obj) {
        obj.SetActive(false);
        pool_effects.Push(obj);
    }

    public GameObject GetLootFromPool() {
        if (pool_loots.Count > 0) {
            GameObject obj = pool_loots.Pop();
            obj.SetActive(true);
            return obj;
        }
        Debug.Log("Cannot create more loots.");
        return null;
    }

    public void AddLootToPool(GameObject obj) {
        obj.SetActive(false);
        pool_loots.Push(obj);
    }
}
