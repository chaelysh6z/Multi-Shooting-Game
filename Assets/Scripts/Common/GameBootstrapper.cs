using UnityEngine;

/// <summary>
/// �� ���� �� ��Ʈ��ũ �Ŵ����� Ǯ �Ŵ����� �ʱ�ȭ/�����ϴ� ��Ʈ��Ʈ�� ��ũ��Ʈ
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerPrefab;
    [SerializeField] private GameObject poolManagerPrefab;

    private void Awake()
    {
        // ��Ʈ��ũ �Ŵ����� ���ٸ� ����
        if (NetworkManager.Instance == null)
        {
            Instantiate(networkManagerPrefab);
        }
    
        // Ǯ �Ŵ����� ���ٸ� ����
        if (PoolManager.Instance == null)
        {
            Instantiate(poolManagerPrefab);
        }
    }
}
