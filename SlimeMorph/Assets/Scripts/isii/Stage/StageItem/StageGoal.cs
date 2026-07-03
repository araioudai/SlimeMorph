using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class StageGoal : MonoBehaviour
{



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // ゴールに到達したときの処理
            Debug.Log("Goal Reached!");
            IT_GameManager.Instance.isGoal = true;

            if (other.gameObject.TryGetComponent(out IT_Player player))
            {
                player.ReachGoal(); // プレイヤーにゴール到達を通知
            }

            GoalCamera goalCamera = FindFirstObjectByType<GoalCamera>();
            if (goalCamera != null)
            {
                goalCamera.StartGoalSequence(other.transform);
            }

            // ステージクリアの処理を非同期で実行
            ClearStageAsync().Forget();
        }
    }

    private async UniTask ClearStageAsync()
    {
        // クリアエフェクトの再生やスコアの計算など、ステージクリアの処理をここに実装
        Debug.Log("Stage Clear!");

        // 例: 3秒待ってから次のステージへ遷移
        await UniTask.Delay(3000);

        // 次のステージへ遷移する処理をここに実装
        SceneManager.LoadScene("I_05TESTG");
    }
}
