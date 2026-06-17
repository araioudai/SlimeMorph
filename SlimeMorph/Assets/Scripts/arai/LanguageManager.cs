using UnityEngine;
using System;

public class LanguageManager : MonoBehaviour
{
    #region シングルトン
    //シングルトン他のスクリプトからいつでもアクセスできるようにする
    public static LanguageManager Instance { get; private set; }

    #endregion

    #region 列挙対
    //言語を定義する列挙型
    public enum Language
    {
        ENGLISH,
        JAPAN,

        MAX
    }

    #endregion

    // PlayerPrefsで使うためのキー名
    private const string LANGUAGE_KEY = "Selected_Language_SlimeMorph";

    //言語が切り替わったときに、ゲーム内の全UI（ボタンやテキスト）へ一斉通知
    public static event Action<Language> OnLanguageChanged;

    //現在選択されている言語を、他のスクリプトから「読み取り専用」で取得用プロパティ
    public Language CurrentLanguage { get; private set; }

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

        //このデータ管理オブジェクトが消えないようにする
        DontDestroyOnLoad(gameObject);

        //ゲーム起動時に、前回保存された言語設定（またはスマホの本体設定）をロードして適用
        CurrentLanguage = LoadLanguage();
    }

    #endregion

    #region
    /// <summary>
    /// 外部（UIボタンなど）から呼ばれる、言語を変更・保存するためのメイン関数
    /// </summary>
    /// <param name="lang">新しく設定したい言語</param>
    public void SetLanguage(Language lang)
    {
        //メモリ上の「現在の言語」を変数に上書き
        CurrentLanguage = lang;

        //端末（PlayerPrefs）にint型（0か1）として保存する
        PlayerPrefs.SetInt(LANGUAGE_KEY, (int)lang);
        PlayerPrefs.Save(); //データを確実に即時書き込み

        //このイベントを登録（購読）しているすべてのUIスクリプトに向けて、
        //言語が新しくなったことを一斉に飛ばす
        OnLanguageChanged?.Invoke(lang);
    }

    /// <summary>
    /// 設定データを読み込むための内部関数
    /// </summary>
    /// <returns>ロードされた言語データ</returns>
    private Language LoadLanguage()
    {
        //すでに一度でも言語を変更したことがあり、保存データが存在する場合
        if (PlayerPrefs.HasKey(LANGUAGE_KEY))
        {
            //保存されているint値（0か1）をLanguage型に変換して返す
            return (Language)PlayerPrefs.GetInt(LANGUAGE_KEY);
        }

        //初回起動などでデータが何も保存されていない場合
        if (Application.systemLanguage == SystemLanguage.English)
        {
            return Language.ENGLISH;
        }
        else
        {
            return Language.JAPAN;
        }
    }
    #endregion
}