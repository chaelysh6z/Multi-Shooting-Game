using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������Ʈ Ǯ���� �����ϴ� �Ŵ��� Ŭ����
/// PoolType���� �̸� ������ ������Ʈ�� �����Ͽ� ������ ����ȭ
/// </summary>
public class PoolManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    private static PoolManager instance;
    public static PoolManager Instance => instance;

    // PoolType�� ������Ʈ ť
    private Dictionary<PoolType, Queue<GameObject>> poolDictionary;
    // PoolType�� ������ ����
    private Dictionary<PoolType, GameObject> prefabDictionary;

    private void Awake()
    {
        // �̱��� �ʱ�ȭ �� ����
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

        // Resources/PoolData �������� PoolData ���µ��� �ε�
        PoolData[] poolDatas = Resources.LoadAll<PoolData>("PoolData");

        // PoolData�� ������� �� PoolType�� ������Ʈ ����
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
    /// ������ PoolType�� ������Ʈ�� ��ȯ
    /// ��� ������ ������Ʈ�� ������ ���� ����
    /// </summary>
    /// <param name="poolType"></param>
    /// <returns></returns>
    public GameObject MakeObj(PoolType poolType)
    {
        if (!poolDictionary.ContainsKey(poolType))
            return null;

        var pool = poolDictionary[poolType];

        // ��Ȱ��ȭ�� ������Ʈ�� ������ ����
        foreach (var obj in pool)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // ��� ������̸� �� ������Ʈ ���� �� Ǯ�� �߰�
        var prefab = prefabDictionary[poolType];
        var newObj = Instantiate(prefab, transform);
        newObj.SetActive(true);
        pool.Enqueue(newObj);
        return newObj;
    }

    /// <summary>
    /// Ư�� PoolType�� ť�� ��ȯ
    /// </summary>
    /// <param name="poolType"></param>
    /// <returns></returns>
    public Queue<GameObject> GetPool(PoolType poolType)
    {
        return poolDictionary.TryGetValue(poolType, out var queue) ? queue : null;
    }

    /// <summary>
    /// ��� Ǯ�� ������Ʈ�� ��Ȱ��ȭ
    /// ���� ���� �� ���
    /// </summary>
    public void DisableAllObjects()
    {
        foreach (var kvp in poolDictionary) // Dictionary ��ȸ
        {
            foreach (var obj in kvp.Value) // Queue ���� GameObject ��ȸ
            {
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}