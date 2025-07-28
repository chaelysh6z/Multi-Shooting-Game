using UnityEngine;

/// <summary>
/// ������Ʈ Ǯ���� ����� �����հ� �ʱ� ���� ���� �����͸� ��� ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "PoolData", menuName = "Scriptable Objects/PoolData")]
public class PoolData : ScriptableObject
{
    public PoolType poolType;
    public GameObject prefab;
    public int size;
}
