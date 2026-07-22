using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

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

        //フェード処理
        LiftFade();
    }

    #endregion

    #region Start呼び出し関数
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

    #region データ保存処理

    /// <summary>
    /// ゲーム終了時の最新データを取得し、GASサーバーへ保存
    /// </summary>
    public void SaveGameResult()
    {
        isSaving = true;
        isSaveComplete = false;

        //各マネージャーおよびPlayerPrefsから最新のゲームデータを取得
        int currentCoin = PlayerPrefs.GetInt("UserCoin", 0);
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

        //保存が終わっていれば（あるいは元から終わっていれば）即座にシーン遷移
        SceneManager.LoadScene(nextSceneName);
    }

    #endregion
}