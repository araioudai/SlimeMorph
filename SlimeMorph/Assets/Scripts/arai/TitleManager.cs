using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Mesh;
using static UnityEngine.Rendering.DebugUI;

public class TitleManager : MonoBehaviour
{
    #region 列挙対
    //ログインか登録か
    private enum Input
    {
        LOGIN,
        REGISTER,

        MAX
    }

    //現在のテキストの状態
    private enum StatusState
    {
        None,
        InputNullBoth,
        InputNullName,
        InputNullPassword,
        Connecting,
        Registering,
        LoginFailed,
        RegisterFailed,
        LoginSuccess,
        RegisterSuccess
    }

    #endregion

    #region private変数
    [Header("ログイン/アカウント作成関連")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject createAccountPanel;
    [Header("入力")]
    [SerializeField, EnumIndex(typeof(Input))] 
    private TMP_InputField[] nameInput = new TMP_InputField[(int)Input.MAX]; //名前入力欄
    [SerializeField, EnumIndex(typeof(Input))] 
    private TMP_InputField[] passInput = new TMP_InputField[(int)Input.MAX]; //パスワード入力欄
    [Header("状態テキスト")]
    [SerializeField, EnumIndex(typeof(Input))] 
    private TMP_Text[] statusText = new TMP_Text[(int)Input.MAX];            //「ログイン中...」などの状態表示
/*    [Header("ロード画面用パネル")]
    [SerializeField] private GameObject loadingPanel;     //ロード中に出すパネル*/

    [Header("待機画面関連")]
    [SerializeField] private GameObject standPanel;

    [Header("強化画面関連")]
    [SerializeField] private GameObject growPanel;

    [Header("スキン画面関連")]
    [SerializeField] private GameObject skinPanel;
    [SerializeField] private SkinListController skinController;

    [Header("設定画面関連")]
    [SerializeField] private GameObject settingPanel;

    [Header("フェード処理関連")]
    [Header("マスクデータ")]
    [SerializeField] private MaskData data;
    [Header("フェード用スクリプト")]
    [SerializeField] private UIShaderFader fader;

    //ログインと登録、それぞれの現在のステータス状態を記憶する配列
    private StatusState[] currentStates = new StatusState[(int)Input.MAX];
    private Coroutine nullMessageCoroutine; //コルーチンの二重動作防止用

    #endregion

    #region Unityイベント関数
    void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //初期化
        Init();

        //ログイン画面分け
        DrawLogin();

        //フェード処理
        ShaderFade();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //パネルがアクティブになった時に言語変更イベントを登録
    void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    //パネルが非アクティブになった時にイベントを解除
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
        //ログイン、登録それぞれの現在の状態に合わせてテキストを再描画
        ApplyStatusText((int)Input.LOGIN, currentStates[(int)Input.LOGIN]);
        ApplyStatusText((int)Input.REGISTER, currentStates[(int)Input.REGISTER]);
    }

    /// <summary>
    /// 指定された状態（State）に応じたテキストとフォントサイズを適用（翻訳の集約場所）
    /// </summary>
    private void ApplyStatusText(int value, StatusState state)
    {
        currentStates[value] = state; //状態を記憶
        TMP_Text targetText = statusText[value];
        bool isEnglish = (LanguageManager.Instance.CurrentLanguage == LanguageManager.Language.ENGLISH);

        switch (state)
        {
            case StatusState.None:
                targetText.text = "";
                break;

            case StatusState.InputNullBoth:
                targetText.fontSize = isEnglish ? 50 : 50;
                targetText.text = isEnglish ? "Username and password are required" : "名前とパスワードが入力されてません";
                break;

            case StatusState.InputNullName:
                targetText.fontSize = isEnglish ? 70 : 70;
                targetText.text = isEnglish ? "Username is required" : "名前が入力されてません";
                break;

            case StatusState.InputNullPassword:
                targetText.fontSize = isEnglish ? 70 : 70;
                targetText.text = isEnglish ? "Password is required" : "パスワードが入力されてません";
                break;

            case StatusState.Connecting:
                targetText.fontSize = isEnglish ? 100 : 125;
                StartLoadingAnim(targetText, isEnglish ? "Connecting" : "通信中");
                break;

            case StatusState.Registering:
                targetText.fontSize = isEnglish ? 100 : 125;
                StartLoadingAnim(targetText, isEnglish ? "Registering" : "登録中");
                break;

            case StatusState.LoginFailed:
                targetText.fontSize = isEnglish ? 70 : 70;
                targetText.text = isEnglish ? "Incorrect User ID or password" : "ユーザー名またはパスワードが正しくありません";
                break;

            case StatusState.RegisterFailed:
                targetText.fontSize = isEnglish ? 70 : 70;
                targetText.text = isEnglish ? "That username is already registered" : "そのユーザー名はすでに登録されています";
                break;

            case StatusState.LoginSuccess:
                targetText.fontSize = isEnglish ? 100 : 100;
                targetText.text = isEnglish ? "Login Complete！" : "ログインしました！";
                break;

            case StatusState.RegisterSuccess:
                targetText.fontSize = isEnglish ? 90 : 100;
                targetText.text = isEnglish ? "Registration complete！" : "登録が完了しました！";
                break;
        }
    }
    #endregion

    #region Start呼び出し関数

    #region 初期化
    void Init()
    {
        for(int i = 0; i < (int)Input.MAX; i++)
        {
            statusText[i].text = "";
        }

        loginPanel.SetActive(true);
        createAccountPanel.SetActive(false);
        growPanel.SetActive(false);
        skinPanel.SetActive(false);
        standPanel.SetActive(false);
        settingPanel.SetActive(false);
    }
    #endregion

    /// <summary>
    /// ログイン画面表示分け
    /// </summary>
    void DrawLogin()
    {
        //ログイン済みかどうかで表示を分ける
        if (OnLineManager.Instance.IsLoggedIn)
        {
            //すでにログインIDがあれば、タイトルを表示
            loginPanel.SetActive(false);
            standPanel.SetActive(true);
        }
        else
        {
            //未ログインならログインパネルを表示
            loginPanel.SetActive(true);
            standPanel.SetActive(false);
        }
    }

    #region フェード処理
    void ShaderFade()
    {
        //if (StageIndex.Instance.GetIsFirst()) { return; }

        LiftFade();
    }

    /// <summary>
    /// フェードイン処理
    /// </summary>
    /// <returns></returns>
    private void LiftFade()
    {
        //広がるアニメーション
        StartCoroutine(fader.PlayFadeIn(data.MaskSpeed(MaskData.MaskType.IN)));
    }
    #endregion

    #endregion

    #region Update呼び出し関数

    #endregion

    #region ボタン関連

    #region ログイン、新規登録関連
    /// <summary>
    /// ログインボタンが押された時
    /// </summary>
    public void OnLoginClick()
    {
        SoundManager.Instance.PlaySE(common.SE.Decision);

        //名前とパスワード
        string userName = nameInput[(int)Input.LOGIN].text;
        string password = passInput[(int)Input.LOGIN].text;

        //名前かパスワード未入力メッセージ
        StartCoroutine(InputNullMessage((int)Input.LOGIN, userName, password));

        //名前とパスワードの入力チェック
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) { return; }

        if (LanguageManager.Instance.CurrentLanguage == LanguageManager.Language.ENGLISH)
        {
            statusText[(int)Input.LOGIN].fontSize = 100;
            StartLoadingAnim(statusText[(int)Input.LOGIN], "Connecting");
        }
        else if (LanguageManager.Instance.CurrentLanguage == LanguageManager.Language.JAPAN)
        {
            statusText[(int)Input.LOGIN].fontSize = 125;
            StartLoadingAnim(statusText[(int)Input.LOGIN], "通信中");
        }

        OnLineManager.Instance.Login(userName, password, (success, message) =>
        {
            if (success)
            {
                ApplyStatusText((int)Input.LOGIN, StatusState.LoginSuccess);
                HandleAuthSuccess();
            }
            else
            {
                DOTween.Kill("LoadingDots");
                ApplyStatusText((int)Input.LOGIN, StatusState.LoginFailed);
            }
        });
    }

    /// <summary>
    /// 新規登録ボタンが押された時
    /// </summary>
    public void OnRegisterClick()
    {
        SoundManager.Instance.PlaySE(common.SE.Decision);

        //名前とパスワード
        string userName = nameInput[(int)Input.REGISTER].text;
        string password = passInput[(int)Input.REGISTER].text;

        //名前かパスワード未入力メッセージ
        StartCoroutine(InputNullMessage((int)Input.REGISTER, userName, password));

        //名前とパスワードの入力チェック
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) { return; }

        //各言語ごとの状態表示
        if (LanguageManager.Instance.CurrentLanguage == LanguageManager.Language.ENGLISH)
        {
            statusText[(int)Input.REGISTER].fontSize = 100;
            StartLoadingAnim(statusText[(int)Input.REGISTER], "Registering");
        }
        else if (LanguageManager.Instance.CurrentLanguage == LanguageManager.Language.JAPAN)
        {
            statusText[(int)Input.REGISTER].fontSize = 125;
            StartLoadingAnim(statusText[(int)Input.REGISTER], "登録中");
        }

        OnLineManager.Instance.Register(userName, password, (success, message) =>
        {
            if (success)
            {
                ApplyStatusText((int)Input.REGISTER, StatusState.RegisterSuccess);
                HandleAuthSuccess();
            }
            else
            {
                DOTween.Kill("LoadingDots");
                ApplyStatusText((int)Input.REGISTER, StatusState.RegisterFailed);
            }
        });
    }

    /// <summary>
    /// 登録やログイン時Nullだったらメッセージ表示
    /// </summary>
    /// <param name="value">ログインか登録か</param>
    /// <param name="userName">名前</param>
    /// <param name="password">パスワード</param>
    /// <returns></returns>
    IEnumerator InputNullMessage(int value, string userName, string password)
    {
        if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
        {
            ApplyStatusText(value, StatusState.InputNullBoth);
        }
        else if (string.IsNullOrEmpty(userName))
        {
            ApplyStatusText(value, StatusState.InputNullName);
        }
        else if (string.IsNullOrEmpty(password))
        {
            ApplyStatusText(value, StatusState.InputNullPassword);
        }

        yield return new WaitForSeconds(2.5f);

        ApplyStatusText(value, StatusState.None);
    }

    /// <summary>
    /// 通信開始時に呼ぶアニメーション
    /// </summary>
    /// <param name="targetText">アニメーションさせたいテキスト</param>
    /// <param name="baseMessage">表示したい固定文字</param>
    void StartLoadingAnim(TMP_Text targetText, string baseMessage)
    {
        int dotCount = 0;

        //DOVirtual.DelayedCall を使って 0.5秒おきに呼び出し
        //最後の引数(false)をtrueにすると無限ループ
        Sequence seq = DOTween.Sequence().SetId("LoadingDots"); //IDをセット

        //0.5秒待ってからドットを更新する処理をループさせる
        seq.AppendCallback(() => {
            dotCount = (dotCount + 1) % 4; // 0, 1, 2, 3 の繰り返し

            string visibleDots = new string('.', dotCount);
            string invisibleDots = new string('.', 3 - dotCount);

            //透明なドットを混ぜて全体の幅を維持
            targetText.text = $"{baseMessage}{visibleDots}<color=#00000000>{invisibleDots}</color>";
        });
        seq.AppendInterval(0.5f);
        seq.SetLoops(-1); //無限ループ
    }


    /// <summary>
    /// ログイン・登録成功時の演出
    /// </summary>
    private void HandleAuthSuccess()
    {
        //通信中のドットアニメを停止
        DOTween.Kill("LoadingDots");

        //メッセージを読ませるために0.5秒待ってからフェード開始
        DOVirtual.DelayedCall(0.5f, () =>
        {
            //フェードアウト（画面を閉じる）
            StartCoroutine(fader.PlayFadeOut(data.MaskSpeed(MaskData.MaskType.OUT), () =>
            {
                //画面が閉じきった後の処理
                loginPanel.SetActive(false);
                createAccountPanel.SetActive(false);
                standPanel.SetActive(true);

                //フェードイン（画面を開く）
                StartCoroutine(fader.PlayFadeIn(data.MaskSpeed(MaskData.MaskType.IN)));
            }));
        });
    }

    /// <summary>
    /// アカウント作成移動ボタン押下処理
    /// </summary>
    public void PushCreateAccount()
    {
        SoundManager.Instance.PlaySE(common.SE.Decision);

        ApplyStatusText((int)Input.LOGIN, StatusState.None);
        loginPanel.SetActive(false);
        createAccountPanel.SetActive(true);
    }

    /// <summary>
    /// ログイン画面に戻るボタン押下処理
    /// </summary>
    public void PushBackLogin()
    {
        SoundManager.Instance.PlaySE(common.SE.Decision);

        ApplyStatusText((int)Input.LOGIN, StatusState.None);
        loginPanel.SetActive(true);
        createAccountPanel.SetActive(false);
    }

    /// <summary>
    /// ログアウト処理
    /// </summary>
    public void PushLogout()
    {
        StartCoroutine(fader.PlayFadeOut(data.MaskSpeed(MaskData.MaskType.OUT), () =>
        {
            OnLineManager.Instance.ResetId();                //IDの削除
            PlayerPrefs.DeleteKey("OnlineUserID");           //ユーザ情報を削除する
            PlayerPrefs.DeleteKey("SavedSelectedSkinIndex"); //スキン情報削除
            PlayerPrefs.DeleteKey("UserCoin");               //コイン情報削除
            PlayerPrefs.DeleteKey("Tutorial_Cleared");       //チュートリアルのクリアフラグを削除する

            PlayerPrefs.Save();                              //セーブする
            Debug.Log("ログアウトしました（PlayerPrefsを削除）");

            if (StageIndex.Instance != null)
            {
                StageIndex.Instance.SetFirst(true);
            }

            //画面が閉じきったタイミングでシーン遷移を開始
            StartCoroutine(TitleLoad());
        }));
    }

    /// <summary>
    /// タイトル読み込み用処理
    /// </summary>
    /// <returns></returns>
    IEnumerator TitleLoad()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("TitleScene");
    }

    #endregion

    #region メニュー関連
    /// <summary>
    /// メニューボタン押下処理
    /// </summary>
    public void PushMenu()
    {
        SoundManager.Instance.PlaySE(common.SE.Decision);

        settingPanel.SetActive(true);
        standPanel.SetActive(false);
    }

    /// <summary>
    /// メニューボタン(戻る)押下処理
    /// </summary>
    public void PushMenuBack()
    {
        SoundManager.Instance.PlaySE(common.SE.Cancel);

        settingPanel.SetActive(false);
        standPanel.SetActive(true);
    }

    #endregion

    #region タイトル内完結処理
    /// <summary>
    /// スキンボタン押下処理
    /// </summary>
    public void PushSkin()
    {
        SoundManager.Instance.PlaySE(common.SE.Decision);

        FadeCommon(standPanel, skinPanel, () =>
        {
            if (skinController != null)
            {
                skinController.InitializeSkinList();
            }
        });
    }

    /// <summary>
    /// 強化(育成)ボタン押下処理
    /// </summary>
    public void PushGrow()
    {
        SoundManager.Instance.PlaySE(common.SE.Decision);

        FadeCommon(standPanel, growPanel);
    }

    /// <summary>
    /// 待機画面に戻る
    /// </summary>
    public void PushBackStand()
    {
        SoundManager.Instance.PlaySE(common.SE.Cancel);

        FadeCommon(new GameObject[] { skinPanel, growPanel }, new GameObject[] { standPanel });
    }

    #endregion

    #region タイトル内でのフェード処理の共通化
    /// <summary>
    /// フェード処理共通(オーバーロード)
    /// 1つのGameObject同士を切り替えるためのオーバーロード
    /// </summary>
    void FadeCommon(GameObject hidden, GameObject display, System.Action onPanelOpened = null)
    {
        //単体のときは、配列に包み直し本体のメソッドになげる
        FadeCommon(new GameObject[] { hidden }, new GameObject[] { display }, onPanelOpened);
    }

    /// <summary>
    /// フェード処理共通(本体)
    /// </summary>
    /// <param name="hidden">非表示にするもの</param>
    /// <param name="display">表示にするもの</param>
    /// <param name="onPanelOpened">ラムダ式（Action）を受け取れるように変更</param>
    void FadeCommon(GameObject[] hidden, GameObject[] display, System.Action onPanelOpened = null)
    {
        StartCoroutine(fader.PlayFadeOut(data.MaskSpeed(MaskData.MaskType.OUT), () =>
        {
            //画面が閉じきった後の処理
            foreach(GameObject h in hidden)
            {
                h.SetActive(false);
            }

            foreach(GameObject d in display)
            {
                d.SetActive(true);
            }

            //画面が完全に表示された直後にメソッドを実行
            onPanelOpened?.Invoke();

            //フェードイン（画面を開く）
            StartCoroutine(fader.PlayFadeIn(data.MaskSpeed(MaskData.MaskType.IN)));
        }));
    }

    #endregion

    /// <summary>
    /// ゲーム開始ボタン押下処理
    /// </summary>
    public void PushPlay()
    {
        SoundManager.Instance.PlaySE(common.SE.Decision);

        //フェードアウト処理
        StartCoroutine(fader.PlayFadeOut(data.MaskSpeed(MaskData.MaskType.OUT), () =>
        {
            //スタミナを消費
            StaminaManager.Instance.StaminaConsume();

            //ゲームシーン読み込み
            StartCoroutine(GameLoad());
        }));
    }

    /// <summary>
    /// ゲームシーン読み込み処理
    /// </summary>
    /// <returns></returns>
    IEnumerator GameLoad()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("GameScene");
    }

    #endregion
}
