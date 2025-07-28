using UnityEngine;

/// <summary>
/// 씬 시작 시 네트워크 매니저와 풀 매니저를 초기화/생성하는 부트스트랩 스크립트
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerPrefab;
    [SerializeField] private GameObject poolManagerPrefab;

    private void Awake()
    {
        // 네트워크 매니저가 없다면 생성
        if (NetworkManager.Instance == null)
        {
            Instantiate(networkManagerPrefab);
        }
    
        // 풀 매니저가 없다면 생성
        if (PoolManager.Instance == null)
        {
            Instantiate(poolManagerPrefab);
        }
    }
}
