using System;
using UnityEngine;

/// <summary>
/// 展示会場向けの、通信を一切行わないプレイヤーデータ管理クラス
/// </summary>
public class OffLineManager : MonoBehaviour, IPlayerBackend
{
    #region シングルトン
    public static OffLineManager Instance { get; private set; }
    #endregion

    #region Unityイベント関数
    void Awake()
    {
        //シングルトン管理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); //既にInstanceがあれば自分を破棄
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        //OffLineManagerが実際に使われる時だけGuestで上書き
        bool isOfflineMode = DebugMode.Instance == null || !DebugMode.Instance.GetDebugMode();
        if (isOfflineMode)
        {
            PlayerPrefs.SetString("UserName", "Guest");
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region プロパティ

    //オフラインなので固定のダミーIDを返す
    public string UserId => "OFFLINE_USER";

    //常にtrueにし、ログイン画面をスキップ
    public bool IsLoggedIn => true;

    #endregion

    /// <summary>
    /// 新規登録(実際の登録処理はしないで、常に成功)
    /// </summary>
    public void Register(string name, string pass, Action<bool, string> onResponse)
    {
        //通信していないので即座にコールバックを返す
        onResponse?.Invoke(true, "OK");
    }

    /// <summary>
    /// ログイン(実際の認証処理はしないで、常に成功)
    /// </summary>
    public void Login(string name, string pass, Action<bool, string> onResponse)
    {
        onResponse?.Invoke(true, "OK");
    }

    /// <summary>
    /// プレイヤーデータの取得(サーバー通信の代わりにローカルのPlayerPrefsからそのまま読む)
    /// </summary>
    public void LoadPlayer(Action<bool, PlayerDataResponse> onResponse)
    {
        //OnLineManagerのLoadPlayerが返すのと同じ形のデータをローカル値から組み立てる
        PlayerDataResponse data = new PlayerDataResponse
        {
            success = true,
            user_id = UserId,
            coin = PlayerPrefs.GetInt("UserCoin", 0),
            clear_stage = PlayerPrefs.GetInt("ClearStage", 0),
            sidespeed_lv = PlayerPrefs.GetInt("GrowLevel_sidespeed_lv", 0),
            defence_lv = PlayerPrefs.GetInt("GrowLevel_defence_lv", 0),
            shrink_lv = PlayerPrefs.GetInt("GrowLevel_shrink_lv", 0),
            stamina = PlayerPrefs.GetInt("Stamina", StaminaManager.Instance != null ? StaminaManager.Instance.MaxStamina : 5),
            recovery_time = PlayerPrefs.GetString("StaminaRecovery", ""),
        };

        //即座にコールバックを返す
        onResponse?.Invoke(true, data);
    }

    /// <summary>
    /// プレイヤーデータの保存
    /// </summary>
    public void SavePlayer(int coin, int sideSpeedLv, int defenceLv, int shrinkLv,
                            int clearStage, int stamina, string recoveryTime,
                            Action<bool> onResponse = null)
    {
        onResponse?.Invoke(true);
    }

    /// <summary>
    /// ログアウト処理
    /// </summary>
    public void ResetId()
    {
        PlayerPrefs.DeleteKey("OnlineUserID");
        PlayerPrefs.Save();
    }
}