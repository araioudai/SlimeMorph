using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

public class ResultManager : MonoBehaviour
{
    #region 変数
    [Header("UI設定")]
    [Tooltip("通信中の表示用オブジェクト")]
    [SerializeField] private GameObject loadingUI;
    [Header("UIボタン")]
    [Tooltip("ゲームクリア表示ボタン")]
    [SerializeField] private GameObject clearButton;
    [Tooltip("ゲームオーバー表示ボタン")]
    [SerializeField] private GameObject overButton;
    [Header("UIテキスト")]
    [SerializeField] private TMP_Text baseCoinText;
    [SerializeField] private TMP_Text multiplierText;
    [SerializeField] private TMP_Text totalCoinText;

    [Header("フェード処理関連")]
    [Header("マスクデータ")]
    [SerializeField] private MaskData data;
    [Header("フェード用スクリプト")]
    [SerializeField] private UIShaderFader fader;

    //通信状態を管理するフラグ
    private bool isSaving = false;       //保存処理中かどうか
    private bool isSaveComplete = false; //保存処理が完了したかどうか

    #endregion

    #region Unityイベント関数

    void Start()
    {
        //ロードUIは初期状態で非表示
        if (loadingUI != null)
        {
            loadingUI.SetActive(false);
        }

        //リザルト画面が開いた瞬間に、裏で自動的にサーバーへデータ保存を開始
        SaveGameResult();

        //リザルトフローを開始
        StartCoroutine(StartResultSequence());
    }

    private void OnDestroy()
    {
        //シーン遷移時にDOTweenアニメーションを安全に破棄
        DOTween.Kill(this);
    }

    #endregion

    #region Start呼び出し関数

    #region リザルトフロー管理

    /// <summary>
    /// フェードイン完了を待ってからリザルト演出を実行する一連の流れ
    /// </summary>
    private IEnumerator StartResultSequence()
    {
        //クリア/オーバーボタンの切り替え
        ScoreResult();

        //フェードインの完了を待つ
        yield return StartCoroutine(fader.PlayFadeIn(data.MaskSpeed(MaskData.MaskType.IN)));

        //画面が完全に表示されたら、DOTweenのカウントアップ演出を開始
        int rawCoins = HandOver.Instance.getCoinCount;
        bool isClear = HandOver.Instance.isGameCleared;

        PlayResultAnimation(rawCoins, isClear);
    }

    #endregion

    private void ScoreResult()
    {
        //ゲームクリア
        if (HandOver.Instance.isGameCleared)
        {
            clearButton.SetActive(true);
            overButton.SetActive(false);
        }
        //ゲームオーバー
        else
        {
            clearButton.SetActive(false);
            overButton.SetActive(true);
        }

        baseCoinText.text = "";
        multiplierText.text = "";
        totalCoinText.text = "";
    }

    /// <summary>
    /// リザルト演出を再生する
    /// </summary>
    /// <param name="rawCoinCount">ゲーム内で拾ったコインの個数 * 10</param>
    /// <param name="isClear">クリアしたかどうか</param>
    private void PlayResultAnimation(int rawCoinCount, bool isClear)
    {
        int multiplier = isClear ? 2 : 1;
        int totalCoins = rawCoinCount * multiplier;

        //初期状態のリセット
        baseCoinText.text = "0";
        multiplierText.text = $"{multiplier}";
        multiplierText.transform.localScale = Vector3.zero; //最初は小さく消しておく
        totalCoinText.text = "0";

        //演出用Sequenceの構築
        Sequence seq = DOTween.Sequence();

        //獲得コインのカウントアップ
        seq.Append(DOVirtual.Float(0, rawCoinCount, 1.0f, value => {
            baseCoinText.text = Mathf.FloorToInt(value).ToString("N0");
        }).SetEase(Ease.OutQuad));

        //ちょっと置く
        seq.AppendInterval(0.2f);

        //倍率が飛び出す演出
        seq.Append(multiplierText.transform.DOScale(1.3f, 0.25f).SetEase(Ease.OutBack));
        seq.Append(multiplierText.transform.DOScale(1.0f, 0.1f));

        seq.AppendInterval(0.2f);

        //合計コインのカウントアップ
        seq.Append(DOVirtual.Float(0, totalCoins, 0.8f, value => {
            totalCoinText.text = Mathf.FloorToInt(value).ToString("N0");
        }).SetEase(Ease.OutQuad));

        //合計表示の最後をポンッと強調
        seq.Append(totalCoinText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 1));
    }

    #endregion

    #region データ保存処理

    /// <summary>
    /// ゲーム終了時の最新データを取得し、GASサーバーへ保存
    /// </summary>
    public void SaveGameResult()
    {
        isSaving = true;
        isSaveComplete = false;

        //今回のプレイ結果から獲得コイン数を計算
        int rawCoins = HandOver.Instance.getCoinCount;
        bool isClear = HandOver.Instance.isGameCleared;
        int multiplier = isClear ? 2 : 1;
        int earnedCoins = rawCoins * multiplier;

        //現在保有コインに今回獲得分を加算
        int currentCoin = PlayerPrefs.GetInt("UserCoin", 0) + earnedCoins;
        PlayerPrefs.SetInt("UserCoin", currentCoin);

        //ローカルのタイムスタンプを保存
        LocalCommon.SaveLocalTimeStamp();

        //PlayerPrefs からその他のステータスを取得
        int sideSpeedLv = PlayerPrefs.GetInt("GrowLevel_sidespeed_lv", 0);
        int defenceLv = PlayerPrefs.GetInt("GrowLevel_defence_lv", 0);
        int shrinkLv = PlayerPrefs.GetInt("GrowLevel_shrink_lv", 0);
        int clearStage = PlayerPrefs.GetInt("ClearStage", 0);

        //StaminaManagerから最新の「スタミナ数」と「次回回復時刻」を取得
        int currentStamina = StaminaManager.Instance.stamina;
        string recoveryTimeStr = StaminaManager.Instance.NextRecoveryTimeISO;

        //OnLineManager経由でGASサーバーへデータを送信
        OnLineManager.Instance.SavePlayer(
            currentCoin,
            sideSpeedLv,
            defenceLv,
            shrinkLv,
            clearStage,
            currentStamina,
            recoveryTimeStr,
            (success) =>
            {
                //通信完了時のコールバック処理
                isSaving = false;
                isSaveComplete = true;

                if (success)
                {
                    Debug.Log("サーバーへのデータ同期が完了しました！");
                }
                else
                {
                    Debug.LogWarning("サーバーへのデータ同期に失敗しました（ローカルデータは保持されています）");
                }
            }
        );
    }

    #endregion

    #region シーン遷移処理（ボタンから呼び出す関数）

    /// <summary>
    /// UIボタン（「タイトルへ」「再プレイ」「次へ」等）から呼び出す共通の画面遷移メソッド
    /// </summary>
    /// <param name="nextSceneName">遷移先のシーン名</param>
    public void OnClickChangeScene(string nextSceneName)
    {
        // 遷移待ちのコルーチンを開始
        StartCoroutine(WaitAndChangeSceneCoroutine(nextSceneName));
    }

    /// <summary>
    /// 通信が完了するまで待機してからシーンを切り替えるコルーチン
    /// </summary>
    /// <param name="nextSceneName">遷移先のシーン名</param>
    private IEnumerator WaitAndChangeSceneCoroutine(string nextSceneName)
    {
        //もしボタンを押した時点でまだ送信中なら、ロード画面を表示
        if (isSaving)
        {
            if (loadingUI != null)
            {
                loadingUI.SetActive(true);
            }

            //保存通信（isSaving）が終わるまでフレーム単位で待機
            while (isSaving)
            {
                yield return null;
            }
        }

        //フェードアウト処理
        StartCoroutine(fader.PlayFadeOut(data.MaskSpeed(MaskData.MaskType.OUT), () =>
        {
            //保存が終わっていれば（あるいは元から終わっていれば）即座にシーン遷移
            SceneManager.LoadScene(nextSceneName);
        }));
    }

    #endregion
}