using SlimeMorph.UI;
using System;
using TMPro;
using UnityEngine;

public class StaminaDisplay : CurrencyDisplay
{
    #region 列挙対
    //現在のテキストの状態
    private enum StatusState
    {
        None,
        StaminaMax,
        StaminaNotMax,
    }

    #endregion

    #region 変数
    [Header("現在のスタミナ表示用テキスト")]
    [SerializeField] private TMP_Text staminaText;

    #endregion

    #region Unityイベント関数
    protected override void Start()
    {
        base.Start();
        
        //起動した最初の数値を表示
        RefreshDisplay();
    }

    private void Update()
    {
        //スタミナ数値とカウントダウンを更新
        RefreshDisplay();
    }

    //パネルがアクティブになった時
    void OnEnable()
    {
        //言語変更イベントを登録
        LanguageManager.OnLanguageChanged += OnLanguageChanged;

        //アクティブ化した瞬間に最新の表示に更新
        RefreshDisplay();
    }

    //パネルが非アクティブになった時
    void OnDisable()
    {
        //イベントの登録を解除
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
    #endregion

    #region 言語切り替えリアルタイム対応
    /// <summary>
    /// 言語設定が切り替わったら呼び出されるイベントハンドラ
    /// </summary>
    private void OnLanguageChanged(LanguageManager.Language newLang)
    {
        //言語が変わったら画面のテキストを再翻訳
        RefreshDisplay();
    }

    /// <summary>
    /// 指定された状態（State）に応じたテキストとフォントサイズを適用する（翻訳の集約場所）
    /// </summary>
    private string ApplyStatusText(StatusState state)
    {
        bool isEnglish = (LanguageManager.Instance.CurrentLanguage == LanguageManager.Language.ENGLISH);

        switch (state)
        {
            case StatusState.None:
                return "";

            case StatusState.StaminaMax:
                return isEnglish ? "FULL" : "FULL";
        }

        return "";
    }

    #endregion

    /// <summary>
    /// スタミナの数値とタイマー表示を最新の状態に更新する共通メソッド
    /// </summary>
    private void RefreshDisplay()
    {
        //シーン上に存在するかチェック
        var manager = StaminaManager.Instance;
        if (manager == null) return;

        //現在のスタミナ数値をUIテキストに反映
        staminaText.text = manager.stamina.ToString();

        //スタミナが満タンかどうかに応じて表示を切り替える
        if (manager.stamina >= manager.MaxStamina)
        {
            //言語設定に対応した「FULL」の文字を表示
            UpdateDisplay(ApplyStatusText(StatusState.StaminaMax));
        }
        else
        {
            //次回回復時刻と現在時刻の差分から残り時間を計算
            TimeSpan timeRemaining = manager.nextRecoveryTime - DateTime.UtcNow;

            //通信ラグやフレームの計算順によって、一瞬でも残り時間がマイナスになるのを防ぐ
            if (timeRemaining.TotalSeconds < 0)
            {
                timeRemaining = TimeSpan.Zero;
            }

            //「分:秒」の形式でタイマー表示を更新
            UpdateDisplay(string.Format("{0:D2}:{1:D2}", timeRemaining.Minutes, timeRemaining.Seconds));
        }
    }
}
