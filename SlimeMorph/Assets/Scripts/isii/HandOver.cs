using UnityEngine;

public class HandOver : MonoBehaviour
{
    #region Singleton
    public static HandOver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Variables
    public bool isGameCleared = false; // true = ゲームクリア、false = ゲームオーバー

    public int getCoinCount = 0; // コインの数をカウントする変数
    #endregion
}
