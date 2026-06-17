using UnityEngine;
using TMPro;

public class LanguageDropdownController : MonoBehaviour
{
    [Header("言語設定用のドロップダウンをセット")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    void Start()
    {
        //現在の言語設定をドロップダウンの表示に同期
        if (LanguageManager.Instance != null)
        {
            //CurrentLanguage(Enum)をintにキャストしてvalueに代入
            languageDropdown.value = (int)LanguageManager.Instance.CurrentLanguage;
        }

        //ドロップダウンの値が変更されたときのイベントを登録
        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
    }
    /// <summary>
    /// ドロップダウンが変更されたときに自動で呼ばれる関数
    /// </summary>
    /// <param name="index">選択された項目の番号 (0 = 英語, 1 = 日本語)</param>
    private void OnLanguageDropdownChanged(int index)
    {
        if (LanguageManager.Instance == null) return;

        //引数のint（0や1）を、LanguageManagerのLanguage型に変換（キャスト）
        LanguageManager.Language selectedLanguage = (LanguageManager.Language)index;

        //言語を変更して保存・UIへ一斉通知
        LanguageManager.Instance.SetLanguage(selectedLanguage);

        Debug.Log($"言語設定を切り替えました: {selectedLanguage}");
    }

    private void OnDestroy()
    {
        //オブジェクト破棄時には念のためイベント登録を解除
        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownChanged);
        }
    }
}