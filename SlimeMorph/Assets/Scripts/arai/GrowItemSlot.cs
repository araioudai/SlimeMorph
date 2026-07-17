using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrowItemSlot : MonoBehaviour
{
    enum GrowState
    {
        Title,
        Explanation,

        Max
    }

    #region private変数
    [Header("ボタン")]
    [SerializeField] private Button button;
    [Header("アイコン画像")]
    [SerializeField] private Image iconImage;
    [Header("説明（タイトル）")]
    [SerializeField] private TMP_Text titleText;
    [Header("説明（詳細）")]
    [SerializeField] private TMP_Text explanationText;

    string[] textEn = new string[(int)GrowState.Max];
    string[] textJa = new string[(int)GrowState.Max];

    //クリックされたことを外部に知らせるイベント（Action）
    public event Action<int> OnClicked;

    //インデックスを外部から読めるように
    public int Index { get; private set; }

    #endregion

    #region Unityイベント関数
    //アクティブになった時に言語変更イベントを登録
    void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;

        //画面に表示された（有効化した）瞬間に、最新の言語で再描画をかける
        if (LanguageManager.Instance != null)
        {
            ApplyStatusText();
        }
    }

    //非アクティブになった時にイベントを解除
    void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    #endregion

    #region 言語切り替えリアルタイム対応
    /// <summary>
    /// 設定画面などで言語が切り替わった瞬間に、現在のエラー表示なども即座に再翻訳する
    /// </summary>
    private void OnLanguageChanged(LanguageManager.Language newLang)
    {
        //テキストを再描画
        ApplyStatusText();
    }

    /// <summary>
    /// 指定された状態（State）に応じたテキストとフォントサイズを適用（翻訳の集約場所）
    /// </summary>
    private void ApplyStatusText()
    {
        if(titleText == null || explanationText == null) { return; }
        
        //まだデータが渡される前であれば処理しない
        if (string.IsNullOrEmpty(textEn[(int)GrowState.Title]) && string.IsNullOrEmpty(textJa[(int)GrowState.Title])) { return; }

        bool isEnglish = (LanguageManager.Instance.CurrentLanguage == LanguageManager.Language.ENGLISH);

        titleText.text = isEnglish ? textEn[(int)GrowState.Title] : textJa[(int)GrowState.Title];
        explanationText.text = isEnglish ? textEn[(int)GrowState.Explanation] : textJa[(int)GrowState.Explanation];
    }
    #endregion

    #region 関数
    /// <summary>
    /// スロットの初期設定、クリックイベントを紐付け
    /// </summary>
    /// <param name="index">スロットに割り当てるインデックス番号</param>
    /// <param name="iconSprite">スロットに表示する画像データ</param>
    /// <param name="title">育成説明タイトル</param>
    /// <param name="explanation">育成説明詳細</param>
    public void Setup(int index, Sprite iconSprite, string titleEn, string titleJa, string explanationEn, string explanationJa)
    {
        //インデックスを保持
        Index = index;

        //テキストデータ保存
        textEn[(int)GrowState.Title] = titleEn;
        textEn[(int)GrowState.Explanation] = explanationEn;

        textJa[(int)GrowState.Title] = titleJa;
        textJa[(int)GrowState.Explanation] = explanationJa;

        //テキストを反映
        ApplyStatusText();

        //画像が渡されていて、Imageコンポーネントが設定されていれば画像を反映
        if (iconImage != null && iconSprite != null)
        {
            iconImage.sprite = iconSprite;
        }

        //ボタンのイベントが重複して登録されるのを防ぐため、一度クリアする
        button.onClick.RemoveAllListeners();

        //ボタンが押されたら、イベントを発火してコントローラーに通知
        button.onClick.AddListener(() => OnClicked?.Invoke(Index));
    }

    #endregion
}
