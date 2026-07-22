using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Data;

#region データ構造定義（JSON変換用データモデル）

/// <summary>
/// 新規登録・ログインの送信用クラス
/// </summary>
[System.Serializable]
public class AuthRequestData
{
    public string action;
    public string name;
    public string password;
}

/// <summary>
/// 新規登録・ログインの受信用クラス
/// </summary>
[System.Serializable]
public class AuthResponseData
{
    public bool success;
    public string message;
    public string user_id;
}

/// <summary>
/// プレイヤーデータ受信用クラス (loadPlayer)
/// </summary>
[System.Serializable]
public class PlayerDataResponse
{
    public bool success;
    public string user_id;
    public string name;
    public int coin;
    public int sidespeed_lv;
    public int defence_lv;
    public int shrink_lv;
    public int clear_stage;
    public int stamina;
    public string recovery_time;
}

/// <summary>
/// プレイヤーデータ送信用クラス (savePlayer)
/// </summary>
[System.Serializable]
public class SavePlayerRequestData
{
    public string action = "savePlayer";
    public string user_id;
    public int coin;
    public int sidespeed_lv;
    public int defence_lv;
    public int shrink_lv;
    public int clear_stage;
    public int stamina;
    public string recovery_time;
}

#endregion

public class OnLineManager : MonoBehaviour
{
    #region シングルトン（他のスクリプトからInstanceでアクセスできるようにする）
    public static OnLineManager Instance { get; private set; }
    #endregion

    #region 変数
    [SerializeField] private string gasUrl = "https://script.google.com/macros/s/AKfycbyoErrYxt1HDWm__Np3_AYSULYf3iJ-oSnFcpVz7zGoHt9Zr-ixGOaf7Cmr6Ud4mQ/exec";

    //内部管理用のID
    private string userId;
    public string UserId => userId;

    //ログイン済みかどうかを確認するプロパティ
    public bool IsLoggedIn => !string.IsNullOrEmpty(userId);
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

        //既にログイン済みならIDが保存されている
        userId = PlayerPrefs.GetString("OnlineUserID", "");
    }
    #endregion

    /// <summary>
    /// 新規ユーザー登録（TitleManagerから呼ばれる）
    /// </summary>
    public void Register(string name, string pass, Action<bool, string> onResponse)
    {
        StartCoroutine(AuthCoroutine("register", name, pass, onResponse));
    }

    /// <summary>
    /// 既存ユーザーでログイン（TitleManagerから呼ばれる）
    /// </summary>
    public void Login(string name, string pass, Action<bool, string> onResponse)
    {
        StartCoroutine(AuthCoroutine("login", name, pass, onResponse));
    }

    /// <summary>
    /// 認証通信の共通コルーチン
    /// </summary>
    private IEnumerator AuthCoroutine(string action, string name, string pass, Action<bool, string> onResponse)
    {
        //送るデータをクラスにまとめる
        AuthRequestData requestData = new AuthRequestData
        {
            action = action, // "register" または "login"
            name = name,
            password = pass
        };

        //クラスをJSON文字列に自動変換
        string json = JsonUtility.ToJson(requestData);

        //サーバーへ通信開始
        using (UnityWebRequest request = new UnityWebRequest(gasUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            //通信成功時の処理
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[{action} レスポンス]: {responseText}");

                //JSONをクラスにデコード
                AuthResponseData responseData = JsonUtility.FromJson<AuthResponseData>(responseText);

                if (responseData.success)
                {
                    //ユーザーIDが返ってきた場合は保存
                    if (!string.IsNullOrEmpty(responseData.user_id))
                    {
                        userId = responseData.user_id;
                        PlayerPrefs.SetString("OnlineUserID", userId);
                        PlayerPrefs.SetString("UserName", name);
                        PlayerPrefs.Save();
                    }

                    //TitleManagerに成功（true）とメッセージを伝える
                    onResponse?.Invoke(true, responseData.message);
                }
                else
                {
                    //サーバー側でエラー（重複エラーやパスワード違いなど）があった場合
                    onResponse?.Invoke(false, responseData.message);
                }
            }
            //通信自体が失敗した時（オフラインなど）
            else
            {
                Debug.LogError($"通信エラー: {request.error}");
                onResponse?.Invoke(false, "サーバーとの通信に失敗しました");
            }
        }
    }

    #region データ取得処理
    /// <summary>
    /// サーバーからプレイヤーデータを取得する
    /// </summary>
    public void LoadPlayer(Action<bool, PlayerDataResponse> onResponse)
    {
        if (!IsLoggedIn)
        {
            onResponse?.Invoke(false, null);
            return;
        }
        StartCoroutine(LoadPlayerCoroutine(onResponse));
    }

    private IEnumerator LoadPlayerCoroutine(Action<bool, PlayerDataResponse> onResponse)
    {
        //GET通信なので、URLの後ろにパラメータを結合する
        string url = $"{gasUrl}?action=loadPlayer&user_id={userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[LoadPlayer レスポンス]: {responseText}");

                //JSONをPlayerDataResponseクラスにデコード
                PlayerDataResponse responseData = JsonUtility.FromJson<PlayerDataResponse>(responseText);

                if (responseData.success)
                {
                    //コイン数、クリアステージなど必要なものをローカル（PlayerPrefs）にも保存（キャッシュ）しておく
                    PlayerPrefs.SetInt("UserCoin", responseData.coin);                    //コイン
                    PlayerPrefs.SetInt("ClearStage", responseData.clear_stage);           //クリアステージ数
                    PlayerPrefs.SetInt("Stamina", responseData.stamina);                  //スタミナ数
                    PlayerPrefs.SetString("StaminaRecovery", responseData.recovery_time); //次のスタミナ回復時間

                    //サーバーから取得した強化レベルを、名前に対応したキーでPlayerPrefsに保存
                    PlayerPrefs.SetInt("GrowLevel_sidespeed_lv", responseData.sidespeed_lv);
                    PlayerPrefs.SetInt("GrowLevel_defence_lv", responseData.defence_lv);
                    PlayerPrefs.SetInt("GrowLevel_shrink_lv", responseData.shrink_lv);

                    PlayerPrefs.Save();

                    //StaminaManager にもサーバーのスタミナ情報を反映
                    if (StaminaManager.Instance != null)
                    {
                        StaminaManager.Instance.SetStaminaData(responseData.stamina, responseData.recovery_time);
                    }

                    onResponse?.Invoke(true, responseData);
                }
                else
                {
                    onResponse?.Invoke(false, null);
                }
            }
            else
            {
                Debug.LogError($"通信エラー: {request.error}");
                onResponse?.Invoke(false, null);
            }
        }
    }

    #endregion

    #region データ保存処理 (savePlayer)

    /// <summary>
    /// プレイヤーの最新データ（所持金、ステータス、ステージ進捗、スタミナ情報など）をサーバーに保存する
    /// </summary>
    /// <param name="coin">現在の所持コイン数</param>
    /// <param name="sideSpeedLv">横移動速度の強化レベル</param>
    /// <param name="defenceLv">防御力の強化レベル</param>
    /// <param name="shrinkLv">縮小の強化レベル</param>
    /// <param name="clearStage">クリア済み最高ステージ番号</param>
    /// <param name="stamina">現在のスタミナ残量</param>
    /// <param name="recoveryTime">次回スタミナ回復予定時刻</param>
    /// <param name="onResponse">通信完了時のコールバック（成功: true, 失敗: false）</param>
    public void SavePlayer(int coin, int sideSpeedLv, int defenceLv, int shrinkLv, int clearStage, int stamina, string recoveryTime, Action<bool> onResponse = null)
    {
        //未ログイン状態の場合はサーバー保存を行わずに終了する
        if (!IsLoggedIn)
        {
            Debug.LogWarning("未ログインのためサーバー保存をスキップします");
            onResponse?.Invoke(false); //失敗扱いとしてコールバックを返す
            return;
        }

        //送信用のデータオブジェクトを作成
        SavePlayerRequestData requestData = new SavePlayerRequestData
        {
            action = "savePlayer",       //GAS側で分岐判定に使用するアクション名
            user_id = userId,            //ログイン中のユーザーID
            coin = coin,
            sidespeed_lv = sideSpeedLv,
            defence_lv = defenceLv,
            shrink_lv = shrinkLv,
            clear_stage = clearStage,
            stamina = stamina,
            recovery_time = recoveryTime
        };

        //非同期通信を開始
        StartCoroutine(SavePlayerCoroutine(requestData, onResponse));
    }

    /// <summary>
    /// プレイヤーデータ保存のHTTP POST通信を実行するコルーチン
    /// </summary>
    /// <param name="requestData">送信するプレイヤーデータ構造体</param>
    /// <param name="onResponse">通信結果を呼び出し元に通知するコールバック</param>
    private IEnumerator SavePlayerCoroutine(SavePlayerRequestData requestData, Action<bool> onResponse)
    {
        //送信オブジェクトをJSON文字列に変換
        string json = JsonUtility.ToJson(requestData);

        //指定したGASのURLへPOSTリクエストを作成
        using (UnityWebRequest request = new UnityWebRequest(gasUrl, "POST"))
        {
            //JSON文字列をバイト配列に変換してアップロードハンドラーにセット
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            //ヘッダーにContent-Typeを設定
            request.SetRequestHeader("Content-Type", "application/json");

            //サーバーからのレスポンスを待機
            yield return request.SendWebRequest();

            //通信結果の判定
            if (request.result == UnityWebRequest.Result.Success)
            {
                //通信成功時：レスポンスログを出力し、成功（true）を通知
                Debug.Log($"[SavePlayer レスポンス]: {request.downloadHandler.text}");
                onResponse?.Invoke(true);
            }
            else
            {
                //通信失敗時：エラーログを出力し、失敗（false）を通知
                Debug.LogError($"SavePlayer 通信エラー: {request.error}");
                onResponse?.Invoke(false);
            }
        }
    }

    #endregion

    #region ログアウト処理
    /// <summary>
    /// ユーザーIDの削除（ログアウト用）
    /// </summary>
    public void ResetId()
    {
        userId = "";
        PlayerPrefs.DeleteKey("OnlineUserID");
        PlayerPrefs.Save();
    }
    #endregion
}