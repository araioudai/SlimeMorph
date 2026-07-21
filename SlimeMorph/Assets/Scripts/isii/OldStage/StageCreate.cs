using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class StageCreate : MonoBehaviour
{
    [Header("ステージオブジェクトのリスト")]
    public StageObjectList stageObjectList;

    [Header("使用するステージID")]
    [SerializeField] int StageID = 0; // 現在のステージID
    float blockLength = 0f; // ブロックの長さ
    
    [Header("ステージオブジェクトの親")]
    [SerializeField] Transform stageParent; // ステージオブジェクトの親

    void Start()
    {
        Loading();
    }

    void Loading()
    {
        // プレイヤーの位置に応じてステージを生成する処理
        CSVLoader csvLoader = GetComponent<CSVLoader>();
        List<string[]> csvData = csvLoader.LoadCSV("Stage"); // CSVファイル

        // 開始オブジェクトの生成
        if (stageObjectList.startObject != null)
        {
            if (stageObjectList.startObject.TryGetComponent(out StageBlock startBlock))
            {
                Instantiate(stageObjectList.startObject, Vector3.zero, Quaternion.identity, stageParent);
                blockLength += startBlock.blockLength;      // ブロックの長さを更新
            }
        }

        // CSVデータを元にステージを生成する処理
        foreach (string[] line in csvData)
        {
            if (line.Length == 0) continue;     // 空行をスキップ
            int currentStageID;
            if (!int.TryParse(line[0], out currentStageID)) continue;   // ステージIDのパースに失敗した場合はスキップ
            if (currentStageID != StageID) continue;                    // 現在のステージIDと一致しない場合はスキップ

            for (int i = 1; i < line.Length; i++)
            {
                int objectIndex;
                if (!int.TryParse(line[i], out objectIndex)) continue;                                  // オブジェクトインデックスのパースに失敗した場合はスキップ
                if (objectIndex < 0 || objectIndex >= stageObjectList.stageObjects.Length) continue;    // インデックスが範囲外の場合はスキップ

                if (stageObjectList.stageObjects[objectIndex].TryGetComponent(out StageBlock stageBlock))
                {
                    Instantiate(stageObjectList.stageObjects[objectIndex], new Vector3(0, 0, blockLength), Quaternion.identity, stageParent);
                    blockLength += stageBlock.blockLength;  // ブロックの長さを更新
                }
            }
        }

        // 終了オブジェクトの生成
        if (stageObjectList.endObject != null)
        {
            if (stageObjectList.endObject.TryGetComponent(out StageBlock endBlock))
            {
                Vector3 position = new Vector3(0, 0, blockLength);                      // 縦に並べる
                Instantiate(stageObjectList.endObject, position, Quaternion.identity, stageParent);
                blockLength += endBlock.blockLength;                                    // ブロックの長さを更新
            }
        }
    }




    #region Debug用
    [ContextMenu("RemoveAndCreateStage")]
    public void RemoveAndCreateStage()
    {
        RemoveStage();
        CreateStage();
    }

    [ContextMenu("RemoveStage")]
    public void RemoveStage()
    {
        // 既存のステージオブジェクトを削除する処理
        foreach (Transform child in stageParent)
        {
            Destroy(child.gameObject);
        }
        blockLength = 0f; // ブロックの長さをリセット
    }

    [ContextMenu("CreateStage")]
    public void CreateStage()
    {
        Loading();
    }
    #endregion



}
