using System;
using UnityEngine;

public static class LocalCommon
{
    private const string KEY_LAST_SAVE_TIME = "LastSaveTime";

    /// <summary>
    /// ローカルの最終セーブ日時を現在時刻(UTC)で保存する
    /// </summary>
    public static void SaveLocalTimeStamp()
    {
        PlayerPrefs.SetString(KEY_LAST_SAVE_TIME, DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ローカルの最終セーブ日時を取得する
    /// </summary>
    public static string GetLocalTimeStamp()
    {
        return PlayerPrefs.GetString(KEY_LAST_SAVE_TIME, "");
    }
}