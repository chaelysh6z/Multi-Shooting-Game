using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링을 관리하는 매니저 클래스
/// PoolType별로 미리 생성한 오브젝트를 재사용하여 성능을 최적화
/// </summary>
public class PoolManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static PoolManager instance;
    public static PoolManager Instance => instance;

    // PoolType별 오브젝트 큐
    private Dictionary<PoolType, Queue<GameObject>> poolDictionary;
    // PoolType별 프리팹 참조
    private Dictionary<PoolType, GameObject> prefabDictionary;

    private void Awake()
    {
        // 싱글톤 초기화 및 유지
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();
        prefabDictionary = new Dictionary<PoolType, GameObject>();

        // Resources/PoolData 폴더에서 PoolData 에셋들을 로드
        PoolData[] poolDatas = Resources.LoadAll<PoolData>("PoolData");

        // PoolData를 기반으로 각 PoolType의 오브젝트 생성
        foreach (PoolData poolData in poolDatas)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < poolData.size; i++)
            {
                GameObject obj = Instantiate(poolData.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(poolData.poolType, objectQueue);
            prefabDictionary.Add(poolData.poolType, poolData.prefab);
        }
    }

    /// <summary>
    /// 지정된 PoolType의 오브젝트를 반환
    /// 사용 가능한 오브젝트가 없으면 새로 생성
    /// </summary>
    /// <param name="poolType"></param>
    /// <returns></returns>
    public GameObject MakeObj(PoolType poolType)
    {
        if (!poolDictionary.ContainsKey(poolType))
            return null;

        var pool = poolDictionary[poolType];

        // 비활성화된 오브젝트가 있으면 재사용
        foreach (var obj in pool)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // 모두 사용중이면 새 오브젝트 생성 후 풀에 추가
        var prefab = prefabDictionary[poolType];
        var newObj = Instantiate(prefab, transform);
        newObj.SetActive(true);
        pool.Enqueue(newObj);
        return newObj;
    }

    /// <summary>
    /// 특정 PoolType의 큐를 반환
    /// </summary>
    /// <param name="poolType"></param>
    /// <returns></returns>
    public Queue<GameObject> GetPool(PoolType poolType)
    {
        return poolDictionary.TryGetValue(poolType, out var queue) ? queue : null;
    }

    /// <summary>
    /// 모든 풀의 오브젝트를 비활성화
    /// 게임 리셋 시 사용
    /// </summary>
    public void DisableAllObjects()
    {
        foreach (var kvp in poolDictionary) // Dictionary 순회
        {
            foreach (var obj in kvp.Value) // Queue 안의 GameObject 순회
            {
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}