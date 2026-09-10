using System;

/// <summary>
/// オンライン／オフラインどちらの通信手段でも使える共通インターフェース
/// </summary>
public interface IPlayerBackend
{
    /// <summary>現在ログイン中のユーザーID(オフライン時はダミー値)</summary>
    string UserId { get; }

    /// <summary>ログイン済みかどうか(オフライン時は常にtrueにしてログイン画面を出さない)</summary>
    bool IsLoggedIn { get; }

    /// <summary>
    /// 新規ユーザー登録
    /// </summary>
    /// <param name="name">登録する名前</param>
    /// <param name="pass">登録するパスワード</param>
    /// <param name="onResponse">結果を受け取るコールバック</param>
    void Register(string name, string pass, Action<bool, string> onResponse);

    /// <summary>
    /// 既存ユーザーでログイン
    /// </summary>
    /// <param name="name">ログイン名</param>
    /// <param name="pass">パスワード</param>
    /// <param name="onResponse">結果を受け取るコールバック</param>
    void Login(string name, string pass, Action<bool, string> onResponse);

    /// <summary>
    /// プレイヤーデータを取得する(オンラインならサーバーから、オフラインならローカルから)
    /// </summary>
    /// <param name="onResponse">結果を受け取るコールバック</param>
    void LoadPlayer(Action<bool, PlayerDataResponse> onResponse);

    /// <summary>
    /// プレイヤーデータを保存する(オンラインならサーバーへ、オフラインなら何もしない)
    /// </summary>
    /// <param name="coin">所持コイン数</param>
    /// <param name="sideSpeedLv">横移動速度の強化レベル</param>
    /// <param name="defenceLv">防御力の強化レベル</param>
    /// <param name="shrinkLv">縮小の強化レベル</param>
    /// <param name="clearStage">クリア済み最高ステージ番号</param>
    /// <param name="stamina">現在のスタミナ残量</param>
    /// <param name="recoveryTime">次回スタミナ回復予定時刻</param>
    /// <param name="onResponse">通信完了時のコールバック(成功: true, 失敗: false)</param>
    void SavePlayer(int coin, int sideSpeedLv, int defenceLv, int shrinkLv,
                     int clearStage, int stamina, string recoveryTime,
                     Action<bool> onResponse = null);

    /// <summary>
    /// ユーザーIDの削除(ログアウト用)
    /// </summary>
    void ResetId();
}