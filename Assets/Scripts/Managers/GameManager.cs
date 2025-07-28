using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// ���� ��ü ������ �����ϴ� �Ŵ��� Ŭ����
/// �÷��̾� ����, �������� ����, ���� ����, ���� ���� ó�� ���� ���
/// </summary>
public class GameManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    private static GameManager instance;
    public static GameManager Instance => instance;

    // ���� / ���� �÷��̾� ����
    private Player myPlayer;
    private Player otherPlayer;

    public Player MyPlayer => myPlayer;
    public Player OtherPlayer => otherPlayer;

    private int stage = 1;  // ���� �������� ��ȣ
    private int score = 0;  // ���� ����

    [SerializeField] private Transform[] spawnPoints; // �� ���� ��ġ ���

    private PhotonView photonView;

    private List<Spawn> spawnList;  // ���� ���������� �� ���� ������
    private int spawnIndex;

    // �ؽ�Ʈ ������ �� Ÿ�� ���ڿ��� PoolType���� ����
    private Dictionary<string, PoolType> typeMap = new()
    {
        { "B", PoolType.EnemyB },
        { "S", PoolType.EnemyS },
        { "M", PoolType.EnemyM },
        { "L", PoolType.EnemyL }
    };

    private void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        spawnList = new List<Spawn>();

        myPlayer = NetworkManager.Instance.InstantiatePlayer();

        photonView = GetComponent<PhotonView>();

        // ������ Ŭ���̾�Ʈ���� ���� ����
        if (PhotonNetwork.IsMasterClient)
            NetworkManager.Instance.OpenRoom();
    }

    private void Start()
    {
        // �� ������ �� Ghost Player ��Ȱ��ȭ
        CoroutineUtils.DelayCall(this, 0.1f, DisablePlayer);
    }

    #region Game
    /// <summary>
    /// �������� ���� ��û
    /// ���� ��װ� RPC�� ��� Ŭ���̾�Ʈ�� ����
    /// </summary>
    public void StartStage()
    {
        NetworkManager.Instance.LockRoom();

        photonView.RPC(nameof(RPC_StartStage), RpcTarget.All);

        // Enemy Spawn File Read
        ReadSpawnFile();

        StartCoroutine(SpawnEnemy());
    }

    [PunRPC]
    private void RPC_StartStage()
    {
        myPlayer.InitPlayer(); // RPC�� �ٲ����
        GameUIManager.Instance.ActivateStageUI(stage);
    }

    /// <summary>
    /// �������� ���� ó��
    /// ���� ���������� �Ѿ�ų� ���� Ŭ����
    /// </summary>
    public void EndStage()
    {
        // Stage Increament
        stage++;

        if (stage > 2)
            photonView.RPC(nameof(RPC_GameClear), RpcTarget.All);
        else
        {
            photonView.RPC(nameof(RPC_ClearStage), RpcTarget.All, stage);
            CoroutineUtils.DelayCall(this, 5, StartStage);
        }
    }

    [PunRPC]
    private void RPC_GameClear()
    {
        myPlayer.ActivateController(false);
        GameUIManager.Instance.ActivateGameClearUI(true);
    }

    [PunRPC]
    private void RPC_ClearStage(int nextStage)
    {
        myPlayer.ActivateController(false);
        GameUIManager.Instance.ActivateClearUI(nextStage - 1);

        stage = nextStage;
    }

    /// <summary>
    /// ���� ���� üũ �� ��� Ŭ���̾�Ʈ�� �˸�
    /// </summary>
    public void GameOver()
    {
        if (otherPlayer != null)
        {
            if (!myPlayer.IsDead || !otherPlayer.IsDead) return;
        }

        photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_GameOver()
    {
        myPlayer.ActivateController(false);
        GameUIManager.Instance.ActivateGameOverUI(true);
    }

    /// <summary>
    /// ���� ����� ��û
    /// </summary>
    public void GameRetry()
    {
        photonView.RPC(nameof(RPC_GameRetry), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_GameRetry()
    {
        NetworkManager.Instance.LoadGameScene();
        PoolManager.Instance.DisableAllObjects();
    }
    #endregion

    #region Spawn
    /// <summary>
    /// ���� ������ ����(Stage N.text)�� �о spawnList�� ����
    /// </summary>
    private void ReadSpawnFile()
    {
        // Init
        spawnList.Clear();
        spawnIndex = 0;

        // Read Spawn File
        TextAsset textFile = Resources.Load("Stage/Stage " + stage) as TextAsset;
        StringReader stringReader = new StringReader(textFile.text);

        while (stringReader != null)
        {
            string line = stringReader.ReadLine();
            if (line == null)
                break;

            // Generate Data
            Spawn spawnData = new Spawn();
            string[] datas = line.Split(',');
            spawnData.delay = float.Parse(datas[0]);
            spawnData.type = datas[1];
            spawnData.point = int.Parse(datas[2]);
            spawnList.Add(spawnData);
        }

        // Close Text File
        stringReader.Close();
    }

    /// <summary>
    /// spawnList�� ��ȸ�ϸ� ���� �����ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnEnemy()
    {
        while (spawnIndex < spawnList.Count)
        {
            PoolType type = typeMap[spawnList[spawnIndex].type];
            int enemyPoint = spawnList[spawnIndex].point;
            int viewID = PhotonNetwork.AllocateViewID(true);
            photonView.RPC("RPC_SpawnEnemy", RpcTarget.All, (int)type, enemyPoint, viewID);

            spawnIndex++;

            if (spawnIndex < spawnList.Count)
            {
                yield return new WaitForSeconds(spawnList[spawnIndex].delay);
            }
        }
    }

    [PunRPC]
    private void RPC_SpawnEnemy(int type, int spawnPoint, int viewID)
    {
        PoolType poolType = (PoolType)type;

        GameObject enemy = PoolManager.Instance.MakeObj(poolType);
        if (enemy == null) return;

        enemy.transform.position = spawnPoints[spawnPoint].position;

        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        if (enemyLogic != null) 
            enemyLogic.InitTransform(spawnPoint);

        PhotonView view = enemy.GetComponent<PhotonView>();
        view.ViewID = viewID;
    }
    #endregion

    #region Score
    /// <summary>
    /// ���� ���� ��û
    /// ������ Ŭ���̾�Ʈ������ ó��
    /// </summary>
    /// <param name="newScore"></param>
    [PunRPC]
    public void UpdateScore(int newScore)
    {
        score += newScore;
        GameUIManager.Instance.UpdateScoreUI(score);

        photonView.RPC(nameof(OnUpdateScore), RpcTarget.Others, score);
    }

    [PunRPC]
    private void OnUpdateScore(int updateScore)
    {
        score = updateScore;
        GameUIManager.Instance.UpdateScoreUI(score);
    }

    public void RequestScore(int score)
    {
        photonView.RPC(nameof(UpdateScore), RpcTarget.MasterClient, score);
    }
    #endregion

    #region Player
    /// <summary>
    /// �÷��̾ ���
    /// ����/���� ���ο� ���� myPlayer/otherPlayer ����
    /// </summary>
    /// <param name="player"></param>
    public void RegistPlayer(Player player)
    {
        PhotonView playerView = player.GetComponent<PhotonView>();

        if (playerView.IsMine)
            myPlayer = player;
        else
            otherPlayer = player;
    }

    /// <summary>
    /// Photon �� ������ �� Ư�� ��Ȳ���� Ghost Player�� ���� ���װ� �߻�
    /// ������ Ŭ���̾�Ʈ�� �ƴ� �ٸ� Ŭ���̾�Ʈ������ Ghost Player�� ����
    /// PhotonNetwork.DestroyAll() �� ����ϸ� �÷��̾���� ������� ������
    /// �ӽ÷� Ghost Player�� �������� ��Ȱ��ȭ
    /// </summary>
    public void DisablePlayer()
    {
        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player != myPlayer && player != otherPlayer)
                player.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        }
    }
    #endregion
}