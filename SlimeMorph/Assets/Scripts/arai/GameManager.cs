using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    #region private変数

    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;

    [Header("フェード処理関連")]
    [Header("マスクデータ")]
    [SerializeField] private MaskData data;
    [Header("フェード用スクリプト")]
    [SerializeField] private UIShaderFader fader;

    #endregion

    #region Unityイベント関数
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

    }
    #endregion

    #region Start呼び出し関数

    #region 初期化
    void Init()
    {
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
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
        pausePanel.SetActive(true);
    }

    /// <summary>
    /// ゲームに戻る
    /// </summary>
    public void PushBackPause()
    {
        pausePanel?.SetActive(false);
    }

    /// <summary>
    /// タイトルボタン押下処理
    /// </summary>
    public void PushTitle()
    {
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
