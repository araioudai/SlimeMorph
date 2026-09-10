using UnityEngine;

public class DebugMode : MonoBehaviour
{
    #region シングルトン
    public static DebugMode Instance { get; private set; }
    #endregion

    #region private変数

    [Header("オンラインで使うものセット")]
    [SerializeField] private GameObject[] onLine;
    [Header("オフラインで使うものセット")]
    [SerializeField] private GameObject[] offLine;
    [Header("ture: オンライン／false: 展示用オフライン")]
    [SerializeField] private bool debugMode;

    #endregion

    #region Get関数

    public bool GetDebugMode() { return debugMode; }

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
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    #endregion

    #region Start呼び出し関数

    void Init()
    {
        //オンライン用はdebugModeと同じ状態、オフライン用は反転した状態で表示と非表示
        OnLineObj(debugMode);
        OffLineObj(!debugMode);
    }

    #endregion

    #region 共通関数
    /// <summary>
    /// オフラインで使うものを表示・非表示
    /// </summary>
    /// <param name="isActive">状態</param>
    void OffLineObj(bool isActive)
    {
        if(offLine == null) { return; }

        foreach (GameObject offObj in offLine)
        {
            if (offObj != null) { offObj.SetActive(isActive); }
        }
    }

    /// <summary>
    /// オンラインで使うものを表示・非表示
    /// </summary>
    /// <param name="isActive">状態</param>
    void OnLineObj(bool isActive)
    {
        if (onLine == null) { return; }

        foreach (GameObject onObj in onLine)
        {
            if (onObj != null) { onObj.SetActive(isActive); }
        }
    }

    #endregion
}
