using UnityEngine;

/// <summary>
/// UI 이벤트와 로컬 플레이어 입력을 연결하는 브릿지 역할
/// 조이스틱 및 버튼 입력을 Player 스크립트로 전달
/// 멀티플레이 환경에서 로컬 플레이어만 입력을 받을 수 있도록 처리
/// </summary>
public class InputBridge : MonoBehaviour
{
    public static InputBridge Instance;     // 싱글톤 인스턴스
    private Player localPlayer;             // 현재 로컬 플레이어 참조

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
    /// 로컬 플레이어 등록
    /// </summary>
    public void RegisterPlayer(Player player)
    {
        localPlayer = player;
    }

    /// <summary>
    /// 로컬 플레이어 입력 해제
    /// </summary>
    public void UnregisterPlayer(Player player)
    {
        if (localPlayer == player)
            localPlayer = null;
    }

    #region UI → Player
    public void OnJoyPanel(int type) => localPlayer?.JoyPanel(type);
    public void OnJoyDown() => localPlayer?.JoyDown();
    public void OnJoyUp() => localPlayer?.JoyUp();

    public void OnButtonADown() => localPlayer?.ButtonADown();
    public void OnButtonAUp() => localPlayer?.ButtonAUp();

    public void OnButtonBDown() => localPlayer?.ButtonBDown();
    #endregion
}
