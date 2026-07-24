using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    #region シングルトン（他のスクリプトからInstanceでアクセスできるようにする）
    public static GameManager Instance { get; private set; }
    #endregion

    #region private変数

    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;

    [Header("フェード処理関連")]
    [Header("マスクデータ")]
    [SerializeField] private MaskData data;
    [Header("フェード用スクリプト")]
    [SerializeField] private UIShaderFader fader;

    private bool pause;

    #endregion

    public bool GetPause()
    {
        return pause;
    }

    #region Unityイベント関数
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //初期化
        Init();

        //フェードイン処理
        LiftFade();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(PlayerPrefs.GetInt("ClearStage"));
    }
    #endregion

    #region Start呼び出し関数

    #region 初期化
    void Init()
    {
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
        pause = false;
    }

    /// <summary>
    /// フェードイン処理
    /// </summary>
    /// <returns></returns>
    private void LiftFade()
    {
        //広がるアニメーション
        StartCoroutine(fader.PlayFadeIn(data.MaskSpeed(MaskData.MaskType.IN)));
    }
    #endregion

    #endregion

    #region Update呼び出し関数

    #endregion

    #region ボタン押下処理
    /// <summary>
    /// ポーズボタン押下処理
    /// </summary>
    public void PushPause()
    {
        pause = true;
        Time.timeScale = 0.0f;
        pausePanel.SetActive(true);
    }

    /// <summary>
    /// ゲームに戻る
    /// </summary>
    public void PushBackPause()
    {
        pause = false;
        Time.timeScale = 1.0f;
        pausePanel?.SetActive(false);
    }

    /// <summary>
    /// タイトルボタン押下処理
    /// </summary>
    public void PushTitle()
    {
        //シーン遷移前に時間の進みを元に戻す
        Time.timeScale = 1.0f;

        //フェードアウト処理
        StartCoroutine(fader.PlayFadeOut(data.MaskSpeed(MaskData.MaskType.OUT), () =>
        {
            //ゲームシーン読み込み
            StartCoroutine(TitleLoad());
        }));
    }

    /// <summary>
    /// リザルトボタン押下処理
    /// </summary>
    public void PushResult()
    {
        //シーン遷移前に時間の進みを元に戻す
        Time.timeScale = 1.0f;

        //フェードアウト処理
        StartCoroutine(fader.PlayFadeOut(data.MaskSpeed(MaskData.MaskType.OUT), () =>
        {
            //ゲームシーン読み込み
            StartCoroutine(ResultLoad());
        }));
    }

    /// <summary>
    ///タイトルシーン読み込み処理
    /// </summary>
    /// <returns></returns>
    IEnumerator TitleLoad()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("TitleScene");
    }

    IEnumerator ResultLoad()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("ResultScene");
    }

    #endregion
}
