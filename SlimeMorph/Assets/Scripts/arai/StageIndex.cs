using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageIndex : MonoBehaviour
{
    #region シングルトン（他のスクリプトからInstanceでアクセスできるようにする）
    public static StageIndex Instance { get; private set; }
    #endregion

    #region private変数
    private int stageIndex;        //ステージ番号
    private bool firstTime = true; //最初のプレイだったらチュートリアル
    private bool isFirst = true;   //最初ならフェード処理しないフラグ
    #endregion

    #region Set関数
    /// <summary>
    /// ステージ番号セット
    /// </summary>
    /// <param name="index">現在のステージ番号</param>
    public void SetIndex(int index) { stageIndex = index; }

    /// <summary>
    /// ステージ番号を次へ（ランキングパネルや次のステージへなど）
    /// </summary>
    /// <param name="index">現在のステージ番号</param>
    public void SetNextIndex(int index) { stageIndex += index; if (stageIndex > 14) stageIndex = 1; }

    /// <summary>
    /// 最初のプレイかどうかセット用
    /// </summary>
    /// <param name="first"></param>
    public void SetFirst(bool first) { firstTime = first; }

    /// <summary>
    /// 最初（起動時）フェード処理しないセット用
    /// </summary>
    /// <param name="first">最初かどうか</param>
    /// <returns></returns>
    public void SetIsFirst(bool first) { isFirst = first; }
    #endregion

    #region Get関数
    /// <summary>
    /// ステージ番号入手用
    /// </summary>
    /// <returns>現在のステージ番号</returns>
    public int GetIndex() { return stageIndex; }

    /// <summary>
    /// 最初のプレイかどうか入手用
    /// </summary>
    /// <returns></returns>
    public bool GetFirst() {  return firstTime; }

    /// <summary>
    /// 最初（起動時）かどうか入手用
    /// </summary>
    /// <returns></returns>
    public bool GetIsFirst() { return isFirst; }
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
        //LoadTutorialProgress();

        //初期（ローカルに保存されている前回のステージクリア数を代入、無ければ1）
        stageIndex = PlayerPrefs.GetInt("ClearStage", 0);

        //サーバーから最新のステージクリア数を非同期で取得
        if (OnLineManager.Instance != null)
        {
            OnLineManager.Instance.LoadPlayer((success, playerData) =>
            {
                if (success && playerData != null)
                {
                    //サーバーから無事に取得できたら、ステージクリアを代入
                    stageIndex = playerData.clear_stage;
                }
                else
                {
                    Debug.LogWarning("サーバーからのステージデータ取得に失敗しました。");
                }
            });
        }
    }

    /// <summary>
    /// オンライン用：PlayerPrefsからチュートリアル完了状態を読み込む
    /// </summary>
/*    private void LoadTutorialProgress()
    {
        //すでにクリアしていれば、firstTimeをfalse にしてチュートリアルをスキップ
        if (PlayerPrefs.GetInt("Tutorial_Cleared", 0) == 1)
        {
            firstTime = false;
        }
        else
        {
            firstTime = true;
        }
    }*/
    #endregion

    #region クリアステージ更新・保存処理

    /// <summary>
    /// ステージクリア時に呼び出し、最高クリアステージを更新・保存する
    /// </summary>
    /// <param name="clearedStageIndex">今回クリアしたステージ番号</param>
    public void UpdateClearStage(int clearedStageIndex)
    {
        //現在保存されている最高クリアステージを取得
        int currentMaxClear = PlayerPrefs.GetInt("ClearStage", 0);

        //今回クリアしたステージが、これまでの最高記録を超えている場合
        int nextStage = clearedStageIndex + 1;

        if (nextStage > currentMaxClear)
        {
            PlayerPrefs.SetInt("ClearStage", nextStage);
            PlayerPrefs.Save(); //確実にディスクに書き込む
            Debug.Log($"最高クリアステージを更新しました: {nextStage}");
        }
    }

    #endregion
}
