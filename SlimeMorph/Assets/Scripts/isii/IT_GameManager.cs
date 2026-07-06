using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IT_GameManager : MonoBehaviour
{
    #region Singleton
    public static IT_GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    #region Player Reference
    IT_Player player;
    bool isGameOver = false; // ゲームオーバー状態を管理するフラグ



    #endregion

    [SerializeField] GameObject gameOver;

    [Header("コインの数を表示するテキスト")]
    [SerializeField] Text coinText; // コインの数を表示するテキスト
    int getCoinCount = 0; // コインの数をカウントする変数



    List<StageObjectItem> stageObjects = new(); // ステージオブジェクトのリスト
    [SerializeField] Button stopButton; // 停止ボタンの参照
    [SerializeField] Button resumeButton; // 再開ボタンの参照


    public bool isGoal = false;




    #region Unity Methods
    void Start()
    {
        player = FindFirstObjectByType<IT_Player>();
        coinText.text = "Coins: " + getCoinCount;
        gameOver.SetActive(false); // ゲーム開始時はゲームオーバーキャンバスを非表示にする
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("IT_Playerが見つかりません。");
            return;
        }
        Dead();
    }
    #endregion





    void Dead()
    {
        if (player.GravityValue < 0.5f && !isGameOver)
        {
            Debug.Log("プレイヤーが死亡しました。");
            // ここでゲームオーバー処理を実装する
            player.Die(); // プレイヤーの死亡処理を呼び出す
            gameOver.SetActive(true);
            isGameOver = true;
        }

        if (player.transform.position.y < -10f && !isGameOver)
        {
            Debug.Log("プレイヤーが落下して死亡しました。");
            // ここでゲームオーバー処理を実装する
            player.Die(); // プレイヤーの死亡処理を呼び出す
            gameOver.SetActive(true);
            isGameOver = true;
        }
    }

    public void GetCoin(int amount)
    {
        getCoinCount += amount;
        Debug.Log($"コインを取得しました。現在のコイン数: {getCoinCount}");
        coinText.text = "Coins: " + getCoinCount;
    }

    public void ResetGame()
    {
        // ゲームをリセットする処理をここに実装する
        // 例えば、シーンを再ロードするなど
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void RegisterStageObject(StageObjectItem stageObject)
    {
        if (!stageObjects.Contains(stageObject))
        {
            stageObjects.Add(stageObject);
        }
    }

    public void UnregisterStageObject(StageObjectItem stageObject)
    {
        if (stageObjects.Contains(stageObject))
        {
            stageObjects.Remove(stageObject);
        }
    }

    void StageObjectStop()
    {
        foreach (var stageObject in stageObjects)
        {
            if (stageObject != null)
            {
                stageObject.isStop = true;
            }
        }
    }

    void StageObjectResume()
    {
        foreach (var stageObject in stageObjects)
        {
            if (stageObject != null)
            {
                stageObject.isStop = false;
            }
        }
    }


    #region Button Methods

    public void OnStopButtonClicked()
    {
        StageObjectStop();
        stopButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(true);
    }

    public void OnResumeButtonClicked()
    {
        StageObjectResume();
        stopButton.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(false);
    }



    #endregion
}