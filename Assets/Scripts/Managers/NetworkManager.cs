using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 네트워크 룸 연결과 룸 관리, 씬 전환을 담당하는 매니저 클래스
/// Photon.PunCallbacks를 상속받아 네트워크 이벤트를 처리
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    // 싱글톤 인스턴스
    private static NetworkManager instance;
    public static NetworkManager Instance => instance;

    // 로비 접속 성공 시 호출되는 이벤트
    public event Action OnLobbyJoined;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 시작 시 Photon 서버 연결 및 씬 로드 콜백 등록
    /// </summary>
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        SceneManager.sceneLoaded += SceneLoaded;
    }

    /// <summary>
    /// 마스터 서버 연결 후 로비 접속
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// 로비 접속 완료 시 상태 갱신 및 이벤트 호출
    /// </summary>
    public override void OnJoinedLobby()
    {
        OnLobbyJoined?.Invoke();
    }

    /// <summary>
    /// 새로운 방 생성
    /// 기본 최대 플레이어는 2명
    /// </summary>
    /// <param name="text"></param>
    public void CreateRoom(string text)
    {
        PhotonNetwork.CreateRoom(text, new RoomOptions() { MaxPlayers = 2 });
    }

    /// <summary>
    /// 지정된 이름의 방에 접속 시도
    /// </summary>
    /// <param name="text"></param>
    public void JoinRoom(string text)
    {
        PhotonNetwork.JoinRoom(text);
    }

    /// <summary>
    /// 현재 방을 나감
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 방 접속 후 게임 씬으로 전환 및 풀 초기화
    /// </summary>
    public override void OnJoinedRoom()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.LoadLevel("Game");
        PoolManager.Instance.DisableAllObjects();
    }

    /// <summary>
    /// 방 퇴장 후 로비 씬으로 전환 및 풀 초기화
    /// </summary>
    public override void OnLeftRoom()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.LoadLevel("Lobby");
        PoolManager.Instance.DisableAllObjects();
    }

    /// <summary>
    /// 상대방이 방을 나갔을 때 처리
    /// 게임 씬 리로드 및 풀 초기화
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {

        PhotonNetwork.IsMessageQueueRunning = false;

        // 상대방이 나가면
        PhotonNetwork.LoadLevel("Game");
        PoolManager.Instance.DisableAllObjects();
    }

    /// <summary>
    /// 게임 씬 로드
    /// 재시작이나 게임오버 시 사용
    /// </summary>
    public void LoadGameScene()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.LoadLevel("Game");
    }

    /// <summary>
    /// 씬 로드 완료 시 메시지 큐 다시 활성화
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    /// <summary>
    /// 네트워크 상에서 플레이어 프리팹을 생성
    /// GameManager에서 불림
    /// </summary>
    /// <returns></returns>
    public Player InstantiatePlayer()
    {
        GameObject go = PhotonNetwork.Instantiate("Network/Player", new Vector3(0, 0, 0), Quaternion.identity);
        Player player = go.GetComponent<Player>();
        return player;
    }

    /// <summary>
    /// 방을 다시 열어 플레이어가 들어올 수 있게 함
    /// 게임이 끝났을 때 사용
    /// </summary>
    public void OpenRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }
    }

    /// <summary>
    /// 방을 잠궈 새로운 플레이어가 들어오지 못하게 함
    /// 게임을 시작했 때 사용
    /// </summary>
    public void LockRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }    
}
