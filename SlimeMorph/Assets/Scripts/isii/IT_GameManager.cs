using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
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


        Init();
    }
    #endregion

    #region Player Reference
    IT_Player player;
    bool isGameOver = false; // ゲームオーバー状態を管理するフラグ
    #endregion

    [SerializeField] GameObject gameOver;

    [Header("コインの数を表示するテキスト")]
    [SerializeField] Text coinText; // コインの数を表示するテキスト
    [SerializeField] TextMeshProUGUI coinTextTMP; // コインの数を表示するTextMeshProUGUI
    int getCoinCount = 0; // コインの数をカウントする変数



    List<StageObjectItem> stageObjects = new(); // ステージオブジェクトのリスト
    [SerializeField] Button stopButton; // 停止ボタンの参照
    [SerializeField] Button resumeButton; // 再開ボタンの参照


    public bool isGoal = false;
    bool isGoalExecuted = false; // ゴール処理が一度だけ実行されるようにするフラグ
    bool isDead = false; // プレイヤーが死亡したかどうかを管理するフラグ

    [Header("ステージプレイヤープレファブの参照")]
    [SerializeField] StagePlayerPrefabs stagePlayerPrefabs; // ステージプレイヤープレファブの参照
    private const string SelectedSkinKey = "SavedSelectedSkinIndex";
    [SerializeField] GameObject playerObject; // プレイヤーのGameObjectを保持する変数


    GameManager gameManager;

    #region Unity Methods
    void Init()
    {
        SkinPlayerSpawn();
        player = FindFirstObjectByType<IT_Player>();
        if (player != null)
        {
            player.Init();
        }
        else
        {
            Debug.LogWarning("IT_Playerが見つかりません。");
        }
        coinText.text = "Coins: " + getCoinCount;
        gameOver.SetActive(false); // ゲーム開始時はゲームオーバーキャンバスを非表示にする
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("IT_Playerが見つかりません。");
            return;
        }
        Dead();

        Goal();

    }
    #endregion

    void SkinPlayerSpawn()
    {
        int savedIndex = PlayerPrefs.GetInt(SelectedSkinKey, 0);
        if (stagePlayerPrefabs != null && stagePlayerPrefabs.prefabs.Count > savedIndex)
        {
            GameObject selectedPrefab = stagePlayerPrefabs.prefabs[savedIndex];
            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, playerObject.transform.position, Quaternion.identity, playerObject.transform);
                Debug.Log($"選択されたプレイヤープレファブを生成しました: {selectedPrefab.name}");
            }
            else
            {
                Debug.LogWarning("選択されたプレイヤープレファブがnullです。");
            }
        }
        else
        {
            Debug.LogWarning("保存されたインデックスが範囲外です。");
        }
    }


    void Goal()
    {
        if (isGoal && !isGoalExecuted)
        {
            isGoalExecuted = true;

            int nowStage = PlayerPrefs.GetInt("ClearStage", 1);

            HandOverSet(true); // ゲームクリア時のデータをHandOverに渡す
            StageIndex.Instance.UpdateClearStage(nowStage);

            Debug.Log("ゴールに到達しました。");
            ClearStageAsync().Forget();
        }
    }

    void HandOverSet(bool isGameCleared)
    {
        HandOver.Instance.isGameCleared = isGameCleared;
        HandOver.Instance.getCoinCount = getCoinCount;
    }




    private async UniTask ClearStageAsync()
    {

        // 例: 3秒待ってから次のステージへ遷移
        await UniTask.Delay(3000);

        // ゴールに到達したときの処理をここに実装
        if (gameManager != null)
        {
            gameManager.PushResult();
        }
        else
        {
            Debug.LogWarning("GameManagerが見つかりません。");
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
        }
    }





    void Dead()
    {
        if (player.PlayerSize.x < 0.1f && !isGameOver && !isDead)
        {
            Debug.Log("プレイヤーが死亡しました。");
            // ここでゲームオーバー処理を実装する
            player.Die(); // プレイヤーの死亡処理を呼び出す
            // gameOver.SetActive(true);
            isGameOver = true;
            isDead = true;

            HandOverSet(false); // ゲームオーバー時のデータをHandOverに渡す

            ClearStageAsync().Forget();
        }

        if (player.transform.position.y < -10f && !isGameOver && !isDead)
        {
            Debug.Log("プレイヤーが落下して死亡しました。");
            // ここでゲームオーバー処理を実装する
            player.Die(); // プレイヤーの死亡処理を呼び出す
            // gameOver.SetActive(true);
            isGameOver = true;
            isDead = true;

            HandOverSet(false); // ゲームオーバー時のデータをHandOverに渡す

            ClearStageAsync().Forget();
        }
    }

    public void GetCoin(int amount)
    {
        getCoinCount += amount;
        Debug.Log($"コインを取得しました。現在のコイン数: {getCoinCount}");
        // coinText.text = "Coins: " + getCoinCount;
        coinTextTMP.text = "Coins: " + getCoinCount;
    }

    public void ResetGame()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.PushTitle();
        }
        else
        {
            Debug.LogWarning("GameManagerが見つかりません。");
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
        }
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