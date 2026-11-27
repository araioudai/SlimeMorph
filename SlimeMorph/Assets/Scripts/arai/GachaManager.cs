using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ガチャのレア度列挙型
/// </summary>
public enum Rarity
{
    N,   //銅
    R,   //銀
    SR,  //金
    SSR, //虹

}

public class GachaManager : MonoBehaviour
{
    #region シングルトン（他のスクリプトからInstanceでアクセスできるようにする）
    public static GachaManager Instance { get; private set; }
    #endregion

    #region 変数
    [Header("UI・演出関連")]
    [SerializeField] private CanvasGroup gachaLightCanvasGroup; //溜め中の発光パネル
    [SerializeField] private Image flashImage;                  //フラッシュ用パネル
    [SerializeField] private TMP_Text gachaText;                //Tap To Startなどを表示するテキスト
    [SerializeField] private Button tapToStartButton;           //画面全体またはガチャ演出開始用のボタン

    [Header("リザルトUI")]
    [SerializeField] private GameObject singleResultPanel;      //単発リザルトパネル
    [SerializeField] private TMP_Text skinNameText;             //スキン名表示テキスト
    [SerializeField] private GameObject multipleResultPanel;    //10連リザルトパネル

    [Header("3Dポジション")]
    [SerializeField] private Transform spawnPoint;              //奥の出現位置
    [SerializeField] private Transform singleCenterPoint;       //単発時の着地中央
    [SerializeField] private Transform[] tenGridPoints;         //10連の配置位置（10個）

    [Header("演出設定")]
    [SerializeField] private float shineTime = 2.0f;            //光をチャージする秒数
    [SerializeField] private GameObject slimePrefab;            //スライムプレハブ

    [Header("レア度別カラー")]
    [SerializeField] private Color colorN = new Color(0.8f, 0.5f, 0.3f);   // 銅（N）
    [SerializeField] private Color colorR = new Color(0.7f, 0.7f, 0.7f);   // 銀（R）
    [SerializeField] private Color colorSR = new Color(1.0f, 0.84f, 0.0f); // 金（SR）
    [SerializeField] private Color colorSSR = new Color(1.0f, 0.3f, 1.0f); // 虹（SSR）

    //生成した演出用スライムのインスタンスを保持し、次回実行時に破棄するためのリスト
    private List<GameObject> activeSlimes = new List<GameObject>();

    private bool isTenGacha = false;    //10連かどうかのフラグ
    private bool waitingForTap = false; //タップ待ち状態かどうか

    #endregion

    #region Unityイベント関数
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Init();

        //Tap To Start ボタンにクリックイベントを登録
        if (tapToStartButton != null)
        {
            tapToStartButton.onClick.AddListener(OnTapToStart);
        }
    }

    void OnDisable()
    {
        if (tapToStartButton != null)
        {
            tapToStartButton.onClick.RemoveListener(OnTapToStart);
        }
    }
    #endregion

    #region Start呼び出し関数

    #region 初期化
    /// <summary>
    /// 初期化処理
    /// </summary>
    void Init()
    {
        gachaLightCanvasGroup.alpha = 0f;
        if (flashImage != null) flashImage.gameObject.SetActive(false);
        if (singleResultPanel != null) singleResultPanel.SetActive(false);
        if (multipleResultPanel != null) multipleResultPanel.SetActive(false);
        gachaText.text = "Tap To Start";
    }

    /// <summary>
    /// 前回のガチャ演出で生成したオブジェクトをすべて削除
    /// </summary>
    void ClearActiveSlimes()
    {
        foreach (var slime in activeSlimes)
        {
            if (slime != null) Destroy(slime);
        }
        activeSlimes.Clear();
    }
    #endregion

    #endregion

    #region 外部呼び出し関数
    /// <summary>
    /// ガチャの種類を保持して待機状態にする処理
    /// </summary>
    /// <param name="isTen">true 10連、false 単発</param>
    public void PrepareGacha(bool isTen)
    {
        isTenGacha = isTen;
        InitDirection();

        // 「Tap To Start」の文字を表示し、点滅アニメーションを開始
        if (gachaText != null)
        {
            gachaText.text = "Tap To Start";
            StartTextBlink();
        }

        //タップ受付を有効化
        if (tapToStartButton != null)
        {
            tapToStartButton.gameObject.SetActive(true);
            tapToStartButton.interactable = true;
        }

        waitingForTap = true;
    }

    void InitDirection()
    {
        gachaLightCanvasGroup.alpha = 0f;
        if (flashImage != null) flashImage.gameObject.SetActive(false);
        if (singleResultPanel != null) singleResultPanel.SetActive(false);
        if (multipleResultPanel != null) multipleResultPanel.SetActive(false);

        ClearActiveSlimes();
    }

    /// <summary>
    /// ガチャ開始ボタンが押された時の処理
    /// </summary>
    void OnTapToStart()
    {
        if (!waitingForTap) return;
        waitingForTap = false;

        //タップ受付ボタンを無効化・非表示にする
        if (tapToStartButton != null)
        {
            tapToStartButton.interactable = false;
            tapToStartButton.gameObject.SetActive(false);
        }

        //テキストの点滅アニメーションを停止し、文字を消す
        if (gachaText != null)
        {
            gachaText.DOKill(); //点滅Tweenを停止
            gachaText.text = "";
        }

        if (SoundManager.Instance != null) { SoundManager.Instance.PlaySE(common.SE.Decision); }

        //ガチャの種類に応じて演出を開始
        if (isTenGacha)
        {
            StartCoroutine(PlayTenGachaSequence());
        }
        else
        {
            StartCoroutine(PlaySingleGachaSequence());
        }
    }

    /// <summary>
    /// テキストの点滅ループ処理
    /// </summary>
    void StartTextBlink()
    {
        if (gachaText == null) return;
        gachaText.color = new Color(gachaText.color.r, gachaText.color.g, gachaText.color.b, 1f);
        gachaText.DOFade(0.0f, 1.0f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId("GachaStartText"); //停止用ID
    }

    #endregion

    #region 単発ガチャ
    /// <summary>
    /// 単発ガチャの演出を実行するコルーチン
    /// </summary>
    private IEnumerator PlaySingleGachaSequence()
    {
        //抽選後レア度を決定する
        Rarity resultRarity = DrawRarity();

        //光のチャージ
        yield return gachaLightCanvasGroup.DOFade(1.0f, shineTime).WaitForCompletion();

        //レア度に応じた色で画面を一瞬フラッシュ
        PlayFlashEffect(resultRarity);

        //スライムが手前へジャンプ着地
        GameObject slime = Instantiate(slimePrefab, spawnPoint.position, Quaternion.identity);
        SetLayerRecursively(slime, LayerMask.NameToLayer("GachaVisual"));
        activeSlimes.Add(slime);
        slime.transform.localScale = Vector3.zero;

        //奥から画面中央へ向かって、移動と拡大を同時に
        Sequence jumpSeq = DOTween.Sequence();
        //山なりにジャンプ
        jumpSeq.Append(slime.transform.DOJump(singleCenterPoint.position, 5.0f, 1, 0.6f).SetEase(Ease.OutQuad));
        jumpSeq.Join(slime.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack));
        yield return jumpSeq.WaitForCompletion();

        //着地した瞬間を表現するアニメーション
        yield return PlaySquishAnimation(slime.transform);

        //ポーズ
        Sequence poseSeq = DOTween.Sequence();
        poseSeq.Append(slime.transform.DOJump(singleCenterPoint.position, 0.8f, 1, 0.4f));
        poseSeq.Join(slime.transform.DORotate(new Vector3(0, 360, 0), 0.4f, RotateMode.FastBeyond360));
        yield return poseSeq.WaitForCompletion();

        //ポーズ着地後のアニメーション
        yield return PlaySquishAnimation(slime.transform);

        //単発リザルト表示(スキン名や獲得画面のUI)
        if (singleResultPanel != null)
        {
            singleResultPanel.SetActive(true);
            if (skinNameText != null) skinNameText.text = $"{resultRarity} スライムスキン";
        }
    }
    #endregion

    #region 10連ガチャ
    /// <summary>
    /// 10連ガチャの演出を実行するコルーチン
    /// </summary>
    private IEnumerator PlayTenGachaSequence()
    {
        //10回分のレア度をまとめて抽選する
        List<Rarity> results = new List<Rarity>();
        for (int i = 0; i < 10; i++) results.Add(DrawRarity());

        //10体の中で一番高いレア度を割り出し、フラッシュやチャージの色に反映させる
        Rarity highestRarity = GetHighestRarity(results);

        //光のチャージ
        yield return gachaLightCanvasGroup.DOFade(1.0f, shineTime).WaitForCompletion();

        //フラッシュを発生させ、チャージ光は薄くして維持する
        PlayFlashEffect(highestRarity);
        gachaLightCanvasGroup.DOFade(0.2f, 0.5f);

        //連続ジャンプ演出（10体を1体ずつテンポよく画面のグリッドに配置していく）
        for (int i = 0; i < results.Count; i++)
        {
            Rarity currentRarity = results[i];

            //用意した10個の配置場所を取得し、足りない場合は奥のスポーン地点にする
            Transform targetPoint = (tenGridPoints != null && i < tenGridPoints.Length)
                ? tenGridPoints[i]
                : spawnPoint;

            //スライムを生成してサイズを0にする
            GameObject slime = Instantiate(slimePrefab, spawnPoint.position, Quaternion.identity);
            SetLayerRecursively(slime, LayerMask.NameToLayer("GachaVisual"));
            activeSlimes.Add(slime);
            slime.transform.localScale = new Vector3();

            //SSRが出たとき、ゲーム全体を一時的にスローモーションに
            if (currentRarity == Rarity.SSR)
            {
                Time.timeScale = 0.3f;                         //時間の経過速度を遅くする
                yield return new WaitForSecondsRealtime(0.4f); //0.4秒待機
                Time.timeScale = 1.0f;                         //時間を通常に戻す
            }

            //奥のスポーン地点から、各自の割り当てられたグリッド位置へポンと飛び出すアニメーション
            Sequence popSeq = DOTween.Sequence();
            popSeq.Append(slime.transform.DOJump(targetPoint.position, 1.5f, 1, 0.35f).SetEase(Ease.Linear));
            popSeq.Join(slime.transform.DOScale(Vector3.one * 0.7f, 0.35f)); //一覧用に少し小さめのサイズにする

            //着地した瞬間のアニメーション
            popSeq.Append(slime.transform.DOScaleY(0.5f, 0.08f));
            popSeq.Append(slime.transform.DOScaleY(0.7f, 0.08f));

            //次のスライムが飛び出すまでのインターバル
            yield return new WaitForSeconds(0.15f);
        }

        //飛び出し終わってから少し余韻を取る
        yield return new WaitForSeconds(0.5f);

        //10連リザルト一覧表示：獲得したスライム一覧パネルをアクティブに
        if (multipleResultPanel != null)
        {
            multipleResultPanel.SetActive(true);
        }
    }
    #endregion

    #region 共通処理関数
    /// <summary>
    /// スライムのぷるぷるを表現するモーション
    /// </summary>
    private IEnumerator PlaySquishAnimation(Transform t)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOScale(new Vector3(1.3f, 0.7f, 1.3f), 0.1f)); //着地して横に広がる
        seq.Append(t.DOScale(new Vector3(0.8f, 1.2f, 0.8f), 0.1f)); //縦に伸びる
        seq.Append(t.DOScale(Vector3.one, 0.1f));                   //通常サイズに戻る
        yield return seq.WaitForCompletion();
    }

    /// <summary>
    /// レア度に応じた色で画面を一瞬フラッシュさせ、すぐにフェードアウトさせる処理
    /// </summary>
    private void PlayFlashEffect(Rarity rarity)
    {
        if (flashImage == null) return;
        flashImage.gameObject.SetActive(true);
        Color c = GetRarityColor(rarity);
        c.a = 1.0f;
        flashImage.color = c;
        //1秒かけて透明になりながら消える
        flashImage.DOFade(0f, 1.0f).OnComplete(() => flashImage.gameObject.SetActive(false));
    }

    /// <summary>
    /// 子要素のレイヤーも変更処理
    /// </summary>
    /// <param name="obj">オブジェクト</param>
    /// <param name="newLayer">変更後レイヤー</param>
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    /// <summary>
    /// 確率に基づいてレア度を1つ抽選（N: 50%, R: 35%, SR: 12%, SSR: 3%）
    /// </summary>
    private Rarity DrawRarity()
    {
        float rand = Random.value;
        if (rand < 0.03f) return Rarity.SSR;
        if (rand < 0.15f) return Rarity.SR;
        if (rand < 0.50f) return Rarity.R;
        return Rarity.N;
    }

    /// <summary>
    /// 複数抽選結果の中から最も高いレア度を判定して返す処理
    /// </summary>
    private Rarity GetHighestRarity(List<Rarity> list)
    {
        Rarity high = Rarity.N;
        foreach (var r in list)
        {
            if (r > high) high = r;
        }
        return high;
    }

    /// <summary>
    /// 指定したレア度に対応するカラーを取得処理
    /// </summary>
    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.SSR: return colorSSR;
            case Rarity.SR: return colorSR;
            case Rarity.R: return colorR;
            default: return colorN;
        }
    }

    #endregion
}
