using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region private変数

    [Header("フェード処理関連")]
    [Header("マスクデータ")]
    [SerializeField] private MaskData data;
    [Header("フェード用スクリプト")]
    [SerializeField] private UIShaderFader fader;

    #endregion

    #region Unityイベント関数
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //初期化
        Init();

        //フェードイン処理
        LiftFade();
    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    #region Start呼び出し関数

    #region 初期化
    void Init()
    {

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
}
