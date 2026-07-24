using System;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    #region シングルトン（他のスクリプトからInstanceでアクセスできるようにする）
    public static StaminaManager Instance { get; private set; }
    #endregion

    #region 変数
    [Header("最大スタミナ数")]
    [SerializeField] private int maxStamina; //最大スタミナ数
    public int MaxStamina => maxStamina;     //最大スタミナ数をプロパティで読み取る

    [Header("1回復するのに必要な時間（分）")]
    [SerializeField] private int recoveryMinutes = 3;
    private TimeSpan recoveryInterval;

    //現在のスタミナ
    public int stamina { get; private set; }

    //次に1回復する時間
    public DateTime nextRecoveryTime { get; private set; }

    //サーバー送信用
    public string NextRecoveryTimeISO => nextRecoveryTime.ToString("o");

    //PlayerPrefsのセーブ用キー名
    private const string KEY_STAMINA = "Stamina";
    private const string KEY_RECOVERY_TIME = "StaminaRecovery";

    #endregion

    #region Unityイベント関数

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //インターバル時間を設定
        recoveryInterval = TimeSpan.FromMinutes(recoveryMinutes);

        //ゲーム起動時にPlayerPrefsからデータをロードする
        LoadStaminaData();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StaminaCount(); //スタミナ回復時間計測処理
    }

    #endregion

    #region 公開メソッド

    /// <summary>
    /// スタミナを1消費する
    /// </summary>
    public void StaminaConsume()
    {
        if (stamina <= 0) { return; }

        //満タン状態から減る場合、最初の次回回復時刻を設定する
        if (stamina == maxStamina)
        {
            nextRecoveryTime = DateTime.UtcNow + recoveryInterval;
        }

        stamina -= 1;

        //変動があったらセーブ
        SaveStaminaData();
    }

    /// <summary>
    /// OnLineManagerからロードしたスタミナ情報を反映させる
    /// </summary>
    /// <param name="serverStamina">サーバー上のスタミナ数</param>
    /// <param name="serverRecoveryTimeStr">サーバー上の次回回復時刻（ISO 8601形式）</param>
    public void SetStaminaData(int serverStamina, string serverRecoveryTimeStr)
    {
        stamina = serverStamina;

        //文字列の回復時刻をDateTime型にして適用
        if (!string.IsNullOrEmpty(serverRecoveryTimeStr) && DateTime.TryParse(serverRecoveryTimeStr, out DateTime parsedTime))
        {
            nextRecoveryTime = parsedTime.ToUniversalTime();
        }
        else
        {
            nextRecoveryTime = DateTime.UtcNow;
        }

        //サーバーの値でローカルキャッシュも同期更新
        SaveStaminaData();
    }

    #endregion

    #region データのセーブ・ロード処理

    /// <summary>
    /// データをPlayerPrefsに保存する
    /// </summary>
    private void SaveStaminaData()
    {
        //スタミナ数
        PlayerPrefs.SetInt(KEY_STAMINA, stamina);

        //DateTime型は、文字列にして保存
        //次回回復時刻
        PlayerPrefs.SetString(KEY_RECOVERY_TIME, nextRecoveryTime.ToString("o"));

        PlayerPrefs.Save();
    }

    /// <summary>
    /// データをPlayerPrefsから読み込む
    /// </summary>
    private void LoadStaminaData()
    {
        //初回起動（セーブデータがない）時は、最大値を入れる
        stamina = PlayerPrefs.GetInt(KEY_STAMINA, maxStamina);

        string savedTimeStr = PlayerPrefs.GetString(KEY_RECOVERY_TIME, "");

        if (!string.IsNullOrEmpty(savedTimeStr))
        {
            nextRecoveryTime = DateTime.Parse(savedTimeStr).ToUniversalTime();
        }
        else
        {
            nextRecoveryTime = DateTime.UtcNow;
        }
    }

    public void StaminaLogOut()
    {
        stamina = maxStamina;
    }

    #endregion

    #region Update呼び出し関数
    /// <summary>
    /// スタミナ回復時間計測処理
    /// </summary>
    void StaminaCount()
    {
        //スタミナが満タンならタイマー計算しない
        if (stamina >= maxStamina) return;

        //次の回復までの残り時間を計算
        TimeSpan timeRemaining = nextRecoveryTime - DateTime.UtcNow;

        //経過時間ぶんループで回復させる
        bool isRecovered = false;
        while (timeRemaining.TotalSeconds <= 0 && stamina < maxStamina)
        {
            stamina++;
            nextRecoveryTime += recoveryInterval;
            timeRemaining = nextRecoveryTime - DateTime.UtcNow; //次の周回の残り時間を再計算
            isRecovered = true;
        }

        //回復が発生した場合は、最新の状態をローカルにセーブ
        if (isRecovered)
        {
            SaveStaminaData();
        }
    }

    #endregion
}
