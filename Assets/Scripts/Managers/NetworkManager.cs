using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ��Ʈ��ũ �� ����� �� ����, �� ��ȯ�� ����ϴ� �Ŵ��� Ŭ����
/// Photon.PunCallbacks�� ��ӹ޾� ��Ʈ��ũ �̺�Ʈ�� ó��
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    // �̱��� �ν��Ͻ�
    private static NetworkManager instance;
    public static NetworkManager Instance => instance;

    // �κ� ���� ���� �� ȣ��Ǵ� �̺�Ʈ
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
    /// ���� �� Photon ���� ���� �� �� �ε� �ݹ� ���
    /// </summary>
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        SceneManager.sceneLoaded += SceneLoaded;
    }

    /// <summary>
    /// ������ ���� ���� �� �κ� ����
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// �κ� ���� �Ϸ� �� ���� ���� �� �̺�Ʈ ȣ��
    /// </summary>
    public override void OnJoinedLobby()
    {
        OnLobbyJoined?.Invoke();
    }

    /// <summary>
    /// ���ο� �� ����
    /// �⺻ �ִ� �÷��̾�� 2��
    /// </summary>
    /// <param name="text"></param>
    public void CreateRoom(string text)
    {
        PhotonNetwork.CreateRoom(text, new RoomOptions() { MaxPlayers = 2 });
    }

    /// <summary>
    /// ������ �̸��� �濡 ���� �õ�
    /// </summary>
    /// <param name="text"></param>
    public void JoinRoom(string text)
    {
        PhotonNetwork.JoinRoom(text);
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// �� ���� �� ���� ������ ��ȯ �� Ǯ �ʱ�ȭ
    /// </summary>
    public override void OnJoinedRoom()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.LoadLevel("Game");
        PoolManager.Instance.DisableAllObjects();
    }

    /// <summary>
    /// �� ���� �� �κ� ������ ��ȯ �� Ǯ �ʱ�ȭ
    /// </summary>
    public override void OnLeftRoom()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.LoadLevel("Lobby");
        PoolManager.Instance.DisableAllObjects();
    }

    /// <summary>
    /// ������ ���� ������ �� ó��
    /// ���� �� ���ε� �� Ǯ �ʱ�ȭ
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {

        PhotonNetwork.IsMessageQueueRunning = false;

        // ������ ������
        PhotonNetwork.LoadLevel("Game");
        PoolManager.Instance.DisableAllObjects();
    }

    /// <summary>
    /// ���� �� �ε�
    /// ������̳� ���ӿ��� �� ���
    /// </summary>
    public void LoadGameScene()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.LoadLevel("Game");
    }

    /// <summary>
    /// �� �ε� �Ϸ� �� �޽��� ť �ٽ� Ȱ��ȭ
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    /// <summary>
    /// ��Ʈ��ũ �󿡼� �÷��̾� �������� ����
    /// GameManager���� �Ҹ�
    /// </summary>
    /// <returns></returns>
    public Player InstantiatePlayer()
    {
        GameObject go = PhotonNetwork.Instantiate("Network/Player", new Vector3(0, 0, 0), Quaternion.identity);
        Player player = go.GetComponent<Player>();
        return player;
    }

    /// <summary>
    /// ���� �ٽ� ���� �÷��̾ ���� �� �ְ� ��
    /// ������ ������ �� ���
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
    /// ���� ��� ���ο� �÷��̾ ������ ���ϰ� ��
    /// ������ ������ �� ���
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
