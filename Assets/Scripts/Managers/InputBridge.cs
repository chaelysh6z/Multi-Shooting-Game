using UnityEngine;

/// <summary>
/// UI �̺�Ʈ�� ���� �÷��̾� �Է��� �����ϴ� �긴�� ����
/// ���̽�ƽ �� ��ư �Է��� Player ��ũ��Ʈ�� ����
/// ��Ƽ�÷��� ȯ�濡�� ���� �÷��̾ �Է��� ���� �� �ֵ��� ó��
/// </summary>
public class InputBridge : MonoBehaviour
{
    public static InputBridge Instance;     // �̱��� �ν��Ͻ�
    private Player localPlayer;             // ���� ���� �÷��̾� ����

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ���� �÷��̾� ���
    /// </summary>
    public void RegisterPlayer(Player player)
    {
        localPlayer = player;
    }

    /// <summary>
    /// ���� �÷��̾� �Է� ����
    /// </summary>
    public void UnregisterPlayer(Player player)
    {
        if (localPlayer == player)
            localPlayer = null;
    }

    #region UI �� Player
    public void OnJoyPanel(int type) => localPlayer?.JoyPanel(type);
    public void OnJoyDown() => localPlayer?.JoyDown();
    public void OnJoyUp() => localPlayer?.JoyUp();

    public void OnButtonADown() => localPlayer?.ButtonADown();
    public void OnButtonAUp() => localPlayer?.ButtonAUp();

    public void OnButtonBDown() => localPlayer?.ButtonBDown();
    #endregion
}
