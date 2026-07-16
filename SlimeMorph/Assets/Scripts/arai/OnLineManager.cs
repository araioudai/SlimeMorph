using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Data;

//送信用データの共通クラス
[System.Serializable]
public class AuthRequestData
{
    public string action;
    public string name;
    public string password;
}

//受信用データの共通クラス
[System.Serializable]
public class AuthResponseData
{
    public bool success;
    public string message;
    public string user_id;
}

//プレイヤーデータ受信用クラス
[System.Serializable]
public class PlayerDataResponse
{
    public bool success;
    public string user_id;
    public string name;
    public int coin;
    public int power_lv;
    public int shrink_lv;
    public int clear_stage;
    public int stamina;
    public string recovery_time;
}

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
                    PlayerPrefs.Save();

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