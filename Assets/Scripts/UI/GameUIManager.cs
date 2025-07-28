using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 내 UI(스코어, 라이프, 스테이지 연출, 게임오버 등)를 관리하는 매니저
/// </summary>
public class GameUIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static GameUIManager instance;
    public static GameUIManager Instance => instance;

    [Header("UI Object")]
    [SerializeField] private Button startButton;        // 게임 시작 버튼
    [SerializeField] private Button lobbyButton;        // 로비로 돌아가는 버튼
    [SerializeField] private Button retryButton;        // 재시작 버튼
    [SerializeField] private TMP_Text scoreText;        // 점수 표시 텍스트
    [SerializeField] private Image[] lifeImage;         // 플레이어 목숨 아이콘
    [SerializeField] private Image[] boomImage;         // 플레이어 폭탄 아이콘
    [SerializeField] private GameObject gameOverText;   // 게임 오버 텍스트
    [SerializeField] private GameObject gameClearText;  // 게임 클리어 텍스트
    [SerializeField] private GameObject boomEffect;     // 폭탄 사용 시 효과

    [Header("Animator")]
    [SerializeField] private Animator stageAnim;        // 스테이지 시작 애니메이션
    [SerializeField] private Animator clearAnim;        // 스테이지 클리어 애니메이션
    [SerializeField] private Animator fadeAnim;         // 화면 전환 페이드 애니메이션

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 마스터 클라이언트만 시작 버튼 활성화
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(OnClickStartButton);
        }

        // 버튼 이벤트 초기화
        lobbyButton.onClick.AddListener(OnClickLobbyButton);
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 해제
        startButton?.onClick.RemoveListener(OnClickStartButton);
        lobbyButton?.onClick.RemoveListener(OnClickLobbyButton);
    }

    /// <summary>
    /// 게임 시작 버튼 클릭 이벤트
    /// </summary>
    private void OnClickStartButton()
    {
        GameManager.Instance.StartStage();
        startButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 로비 버튼 클릭 이벤트
    /// </summary>
    private void OnClickLobbyButton()
    {
        NetworkManager.Instance.LeaveRoom();
    }

    /// <summary>
    /// 스테이지 시작 연출을 활성화
    /// </summary>
    /// <param name="stage"></param>
    public void ActivateStageUI(int stage)
    {
        // Stage UI Load
        stageAnim.GetComponent<TMP_Text>().text = "Stage " + stage + "\nStart";

        stageAnim.SetTrigger("On");

        // Fade In
        fadeAnim.SetTrigger("In");
    }

    /// <summary>
    /// 스테이지 클리어 연출을 활성화
    /// </summary>
    /// <param name="stage"></param>
    public void ActivateClearUI(int stage)
    {
        // Clear UI Load
        clearAnim.GetComponent<TMP_Text>().text = "Stage " + stage + "\nClear!";
        clearAnim.SetTrigger("On");

        // Fade Out
        fadeAnim.SetTrigger("Out");
    }

    /// <summary>
    /// 게임 클리어 UI 활성화
    /// </summary>
    /// <param name="isActivae"></param>
    public void ActivateGameClearUI(bool isActivae)
    {
        gameClearText.SetActive(isActivae);

        if (PhotonNetwork.IsMasterClient)
            retryButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 게임 오버 UI 활성화
    /// </summary>
    /// <param name="isActive"></param>
    public void ActivateGameOverUI(bool isActive)
    {
        gameOverText.SetActive(isActive);

        if (PhotonNetwork.IsMasterClient)
            retryButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 점수 UI 업데이트
    /// </summary>
    /// <param name="score"></param>
    public void UpdateScoreUI(int score)
    {
        // UI Score Update
        scoreText.text = string.Format("{0:n0}", score);
    }

    /// <summary>
    /// 플레이어 목숨 아이콘 업데이트
    /// </summary>
    /// <param name="life"></param>
    public void UpdateLifeUI(int life)
    {
        // UI Life Init Disable
        for (int index = 0; index < lifeImage.Length; index++)
        {
            lifeImage[index].color = new Color(1, 1, 1, 0);
        }

        // UI Life Active
        for (int index = 0; index < life; index++)
        {
            lifeImage[index].color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// 플레이어 폭탄 아이콘 업데이트
    /// </summary>
    /// <param name="boom"></param>
    public void UpdateBoomUI(int boom)
    {
        // UI Boom Init Disable
        for (int index = 0; index < boomImage.Length; index++)
        {
            boomImage[index].color = new Color(1, 1, 1, 0);
        }

        // UI Boom Active
        for (int index = 0; index < boom; index++)
        {
            boomImage[index].color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// 폭발 이펙트를 풀에서 가져와 실행
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="type"></param>
    public void Explosion(Vector3 pos, AircraftType type)
    {
        var go = PoolManager.Instance.MakeObj(PoolType.Explosion);
        var explosion = go.GetComponent<Explosion>();

        go.transform.transform.position = pos;
        explosion.StartExplosion(type);
    }

    /// <summary>
    /// 폭탄 효과 활성화
    /// </summary>
    /// <param name="isActive"></param>
    public void ActivateBoomEffect(bool isActive)
    {
        boomEffect.SetActive(isActive);
    }
}