using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 各言語ごとのエラーメッセージをInspector上で設定するための構造体
/// </summary>
[Serializable]
public struct ErrorMessages
{
    [TextArea(3, 10)]
    public string text;
}

public class ErrorManager : MonoBehaviour
{
    #region シングルトン
    ///静的インスタンス（どこからでも ErrorManager.Instance で呼び出し可能）
    public static ErrorManager Instance { get; private set; }

    private void Awake()
    {
        //インスタンスの重複チェック（シングルトン化）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region 変数
    [Header("エラー画面UI")]
    [Tooltip("エラー表示全体の親パネル")]
    [SerializeField] private GameObject errorPanel;

    [Tooltip("通信をやり直すためのリトライボタン")]
    [SerializeField] private Button retryButton;

    [Tooltip("処理をキャンセルして閉じるボタン")]
    [SerializeField] private Button closeButton;

    [Tooltip("エラー内容を表示するテキスト")]
    [SerializeField] private TMP_Text errorMessageText;

    [Header("多言語メッセージ設定")]
    [SerializeField, EnumIndex(typeof(LanguageManager.Language))]
    private ErrorMessages[] errorMessage = new ErrorMessages[(int)LanguageManager.Language.MAX];
    
    //リトライボタンが押された時に実行する処理（呼び出し元から受け取る）
    private Action onRetryCallback;

    //閉じるボタンが押された時に実行する処理（呼び出し元から受け取る）
    private Action onCloseCallback;
    #endregion

    #region Unityイベント関数
    private void Start()
    {
        //ゲーム起動時はエラーパネルを確実に非表示にしておく
        if (errorPanel != null) { errorPanel.SetActive(false); }

        //ボタンのクリックイベントにリスナーを登録
        if (retryButton != null) { retryButton.onClick.AddListener(OnClickRetry); }
        if (closeButton != null) { closeButton.onClick.AddListener(OnClickClose); }
    }

    private void OnEnable()
    {
        //言語切替イベント
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        //イベント解除
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }
    #endregion

    #region パブリックメソッド（外部からの呼び出し用）

    /// <summary>
    /// エラーダイアログを表示する
    /// </summary>
    /// <param name="onRetry">リトライボタンが押された時の処理（nullの場合はリトライボタンを非表示）</param>
    /// <param name="onClose">閉じるボタンが押された時の処理（ロールバック処理など）</param>
    public void ShowError(Action onRetry = null, Action onClose = null)
    {
        //呼び出し元から渡された処理（コールバック）を保持
        this.onRetryCallback = onRetry;
        this.onCloseCallback = onClose;

        //リトライ処理が指定されていない場合はボタン自体を非表示にする
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(onRetry != null);
        }

        //現在設定されている言語に合わせてテキストを表示
        ApplyStatusText();

        //エラーパネルを表示
        if (errorPanel != null)
        {
            errorPanel.SetActive(true);
        }
    }

    /// <summary>
    /// エラーダイアログを強制的に閉じる
    /// </summary>
    public void HideError()
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(false);
        }
    }

    #endregion

    #region 内部処理・ボタンイベント
    /// <summary>
    /// リトライボタン押下時の処理
    /// </summary>
    private void OnClickRetry()
    {
        //ダイアログを閉じる
        HideError();

        //決定SEの再生
        if (SoundManager.Instance != null) { SoundManager.Instance.PlaySE(common.SE.Decision); }

        //保持しておいたリトライ処理を実行
        onRetryCallback?.Invoke();
    }

    /// <summary>
    /// 閉じる（キャンセル）ボタン押下時の処理
    /// </summary>
    private void OnClickClose()
    {
        //ダイアログを閉じる
        HideError();

        //キャンセルSEの再生
        if (SoundManager.Instance != null) { SoundManager.Instance.PlaySE(common.SE.Cancel); }

        //保持しておいたキャンセル・ロールバック処理を実行
        onCloseCallback?.Invoke();
    }

    /// <summary>
    /// 言語切り替え時に即座にメッセージを再適用する
    /// </summary>
    private void OnLanguageChanged(LanguageManager.Language newLang)
    {
        //パネルが表示中のみテキストを差し替える
        if (errorPanel != null && errorPanel.activeSelf)
        {
            ApplyStatusText();
        }
    }

    /// <summary>
    /// 現在の言語設定に応じてエラーメッセージテキストを更新する
    /// </summary>
    private void ApplyStatusText()
    {
        if (errorMessageText == null || errorMessage == null) return;

        //言語設定の判定
        bool isEnglish = (LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == LanguageManager.Language.ENGLISH);
        int langIndex = isEnglish ? (int)LanguageManager.Language.ENGLISH : (int)LanguageManager.Language.JAPAN;

        //配列の範囲内であればテキストを適用
        if (langIndex < errorMessage.Length)
        {
            errorMessageText.text = errorMessage[langIndex].text;
        }
    }
    #endregion
}