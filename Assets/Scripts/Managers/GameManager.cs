using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 게임 전체 진행을 관리하는 매니저 클래스
/// 플레이어 생성, 스테이지 진행, 점수 관리, 게임 오버 처리 등을 담당
/// </summary>
public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static GameManager instance;
    public static GameManager Instance => instance;

    // 로컬 / 원격 플레이어 참조
    private Player myPlayer;
    private Player otherPlayer;

    public Player MyPlayer => myPlayer;
    public Player OtherPlayer => otherPlayer;

    private int stage = 1;  // 현재 스테이지 번호
    private int score = 0;  // 현재 점수

    [SerializeField] private Transform[] spawnPoints; // 적 스폰 위치 목록

    private PhotonView photonView;

    private List<Spawn> spawnList;  // 현재 스테이지의 적 스폰 데이터
    private int spawnIndex;

    // 텍스트 파일의 적 타입 문자열을 PoolType으로 매핑
    private Dictionary<string, PoolType> typeMap = new()
    {
        { "B", PoolType.EnemyB },
        { "S", PoolType.EnemyS },
        { "M", PoolType.EnemyM },
        { "L", PoolType.EnemyL }
    };

    private void Awake()
    {
        // 싱글톤 초기화
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

        // 마스터 클라이언트에서 방을 개방
        if (PhotonNetwork.IsMasterClient)
            NetworkManager.Instance.OpenRoom();
    }

    private void Start()
    {
        // 씬 재입장 시 Ghost Player 비활성화
        CoroutineUtils.DelayCall(this, 0.1f, DisablePlayer);
    }

    #region Game
    /// <summary>
    /// 스테이지 시작 요청
    /// 방을 잠그고 RPC로 모든 클라이언트에 전달
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
        myPlayer.InitPlayer(); // RPC로 바꿔야함
        GameUIManager.Instance.ActivateStageUI(stage);
    }

    /// <summary>
    /// 스테이지 종료 처리
    /// 다음 스테이지로 넘어가거나 게임 클리어
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
    /// 게임 오버 체크 후 모든 클라이언트에 알림
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
    /// 게임 재시작 요청
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
    /// 스폰 데이터 파일(Stage N.text)을 읽어서 spawnList를 생성
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
    /// spawnList를 순회하며 적을 스폰하는 코루틴
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
    /// 점수 갱신 요청
    /// 마스터 클라이언트에서만 처리
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
    /// 플레이어를 등록
    /// 로컬/원격 여부에 따라 myPlayer/otherPlayer 설정
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
    /// Photon 씬 재입장 시 특정 상황에서 Ghost Player가 남는 버그가 발생
    /// 마스터 클라이언트가 아닌 다른 클라이언트에서만 Ghost Player가 존재
    /// PhotonNetwork.DestroyAll() 을 사용하면 플레이어까지 사라지기 때문에
    /// 임시로 Ghost Player의 렌더링만 비활성화
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