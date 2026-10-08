using DG.Tweening;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    #region 変数
    [SerializeField] private CanvasGroup gachaLightCanvasGroup;

    #endregion

    #region Unityイベント関数
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region Start呼出し関数
    /// <summary>
    /// 初期化処理
    /// </summary>
    void Init()
    {
        gachaLightCanvasGroup.alpha = 0f;
    }

    #endregion

    #region 外部呼出し関数
    /// <summary>
    /// ガチャの演出処理
    /// </summary>
    public void GachaEffect()
    {
        gachaLightCanvasGroup.alpha = 0f;
        gachaLightCanvasGroup.DOFade(1.0f, 1.2f); //1.2秒かけて光らせる
    }

    #endregion
}
