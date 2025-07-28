using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �κ� UI�� �����ϴ� Ŭ����
/// �� ����, �� ���� ��ư �̺�Ʈ ó�� �� �ε� ���� ��ȯ ���
/// </summary>
public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text loadingText;

    [SerializeField] GameObject buttonGroup;

    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    [SerializeField] private TMP_InputField createInputField;
    [SerializeField] private TMP_InputField joinInputField;

    private void Start()
    {
        // �κ� ���� �̺�Ʈ ����
        NetworkManager.Instance.OnLobbyJoined += HandleLobbyJoined;

        createButton.onClick.AddListener(OnClickCreateButton);
        joinButton.onClick.AddListener(OnClickJoinButton);
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� �� ������ ����
        NetworkManager.Instance.OnLobbyJoined -= HandleLobbyJoined;

        createButton?.onClick.RemoveListener(OnClickCreateButton);
        joinButton?.onClick.RemoveListener(OnClickJoinButton);
    }

    /// <summary>
    /// �� ���� ��ư Ŭ�� ��
    /// </summary>
    private void OnClickCreateButton()
    {
        NetworkManager.Instance.CreateRoom(createInputField.text);
    }

    /// <summary>
    /// �� ���� ��ư Ŭ�� ��
    /// </summary>
    private void OnClickJoinButton()
    {
        NetworkManager.Instance.JoinRoom(joinInputField.text);
    }

    /// <summary>
    /// �κ� ����Ǹ� �ε� �ؽ�Ʈ ��Ȱ��ȭ �� ��ư UI Ȱ��ȭ
    /// </summary>
    private void HandleLobbyJoined()
    {
        loadingText.gameObject.SetActive(false);
        buttonGroup.SetActive(true);
    }
}
