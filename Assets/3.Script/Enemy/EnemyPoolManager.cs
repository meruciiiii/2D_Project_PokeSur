using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ObjectPool
{
    private GameObject prefab;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private Transform parentTransform;

    public void Initialize(GameObject prefabToPool, int initialSize, Transform parent)
    {
        this.prefab = prefabToPool;
        this.parentTransform = parent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            ReturnObject(obj);
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject newObj = GameObject.Instantiate(prefab, parentTransform);
        newObj.SetActive(false);
        return newObj;
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 풀이 비었을 경우 동적으로 추가 생성
            Debug.LogWarning("Pool is empty! Creating new object for " + prefab.name);
            GameObject newObj = CreateNewObject();
            newObj.SetActive(true);
            return newObj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(parentTransform);
        pool.Enqueue(obj);
        //Debug.Log("풀 반환 성공: " + obj.name + "이(가) 비활성화되어 풀에 돌아갔습니다.");
    }
}

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject golemPrefab;
    public GameObject mukPrefab;
    public GameObject taurosPrefab;

    [Header("Pool Sizes")]
    public int golemPoolSize = 50;
    public int mukPoolSize = 50;
    public int taurosPoolSize = 50;

    // 개별 풀 인스턴스 (ObjectPool 클래스 사용)
    private ObjectPool golemPool;
    private ObjectPool mukPool;
    private ObjectPool taurosPool;

    // Hierarchy 관리를 위한 부모 Transform
    private Transform poolParent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 씬 전환이 없다면 주석 처리
            InitializePools();
        }
        else
        {
            // 이미 인스턴스가 존재하면 현재 오브젝트 파괴
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        // 몬스터들을 깔끔하게 정리할 부모 오브젝트 생성
        poolParent = new GameObject("EnemyPoolParent").transform;

        // 1. 골렘 풀 초기화
        golemPool = new ObjectPool();
        golemPool.Initialize(golemPrefab, golemPoolSize, poolParent);

        // 2. 묵 풀 초기화
        mukPool = new ObjectPool();
        mukPool.Initialize(mukPrefab, mukPoolSize, poolParent);

        // 3. 타우로스 풀 초기화
        taurosPool = new ObjectPool();
        taurosPool.Initialize(taurosPrefab, taurosPoolSize, poolParent);
    }
    public GameObject GetObject(string monsterName)
    {
        if (monsterName.Equals("Golem"))
        {
            return golemPool.GetObject();
        }
        else if (monsterName.Equals("Muk"))
        {
            return mukPool.GetObject();
        }
        else if (monsterName.Equals("Tauros"))
        {
            return taurosPool.GetObject();
        }

        Debug.LogError("Invalid monster name requested: " + monsterName);
        return null;
    }
    public void ReturnObject(GameObject obj, string monsterName)
    {
        if (monsterName.Equals("Golem"))
        {
            golemPool.ReturnObject(obj);
        }
        else if (monsterName.Equals("Muk"))
        {
            mukPool.ReturnObject(obj);
        }
        else if (monsterName.Equals("Tauros"))
        {
            taurosPool.ReturnObject(obj);
        }
        // 이 외의 오브젝트는 파괴하거나 무시합니다.
    }
}

