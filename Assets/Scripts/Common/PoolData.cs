using UnityEngine;

/// <summary>
/// 오브젝트 풀에서 사용할 프리팹과 초기 생성 개수 데이터를 담는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "PoolData", menuName = "Scriptable Objects/PoolData")]
public class PoolData : ScriptableObject
{
    public PoolType poolType;
    public GameObject prefab;
    public int size;
}
