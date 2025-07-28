using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 UI를 관리하는 클래스
/// 방 생성, 방 입장 버튼 이벤트 처리 및 로딩 상태 전환 담당
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
        // 로비 진입 이벤트 구독
        NetworkManager.Instance.OnLobbyJoined += HandleLobbyJoined;

        createButton.onClick.AddListener(OnClickCreateButton);
        joinButton.onClick.AddListener(OnClickJoinButton);
    }

    private void OnDestroy()
    {
        // 이벤트 해제 및 리스너 제거
        NetworkManager.Instance.OnLobbyJoined -= HandleLobbyJoined;

        createButton?.onClick.RemoveListener(OnClickCreateButton);
        joinButton?.onClick.RemoveListener(OnClickJoinButton);
    }

    /// <summary>
    /// 방 생성 버튼 클릭 시
    /// </summary>
    private void OnClickCreateButton()
    {
        NetworkManager.Instance.CreateRoom(createInputField.text);
    }

    /// <summary>
    /// 방 참가 버튼 클릭 시
    /// </summary>
    private void OnClickJoinButton()
    {
        NetworkManager.Instance.JoinRoom(joinInputField.text);
    }

    /// <summary>
    /// 로비에 연결되면 로딩 텍스트 비활성화 후 버튼 UI 활성화
    /// </summary>
    private void HandleLobbyJoined()
    {
        loadingText.gameObject.SetActive(false);
        buttonGroup.SetActive(true);
    }
}
