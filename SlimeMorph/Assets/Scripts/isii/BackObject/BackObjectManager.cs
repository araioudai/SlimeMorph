using UnityEngine;

public class BackObjectManager : MonoBehaviour
{
    [SerializeField] BackObjectList backObjectList;

    [SerializeField] Transform backObjectParent;

    [SerializeField] int stageId = 1; // 現在のステージID

    void Start()
    {
        int clearStage = PlayerPrefs.GetInt("ClearStage", 1);
        stageId = clearStage + 1; // クリアしたステージに応じてステージIDを設定

        if (stageId > backObjectList.backObjects.Count)
        {
            stageId = Random.Range(0, backObjectList.backObjects.Count + 1); // バックオブジェクトの数を超えた場合はランダムに選択
        }

        LoadBackObjects();
    }

    void LoadBackObjects()
    {
        if (backObjectList == null || backObjectList.backObjects.Count == 0)
        {
            Debug.LogWarning("BackObjectList が設定されていないか、バックオブジェクトがありません。");
            return;
        }

        // stageIdに応じてバックオブジェクトを選択
        if (stageId - 1 < backObjectList.backObjects.Count)
        {
            GameObject backObject = backObjectList.backObjects[stageId - 1];
            if (backObject != null)
            {
                GameObject obj = Instantiate(backObject, backObjectParent);
                obj.transform.localPosition = Vector3.zero; // 必要に応じて位置を調整
            }
            else
            {
                Debug.LogWarning($"ステージID {stageId} のバックオブジェクトが null です。");
            }
        }
        else
        {
            Debug.LogWarning($"ステージID {stageId} のバックオブジェクトが見つかりません。");
        }
    }
}
