using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class IT_AutoStageCreate : MonoBehaviour
{
    [Header("ステージオブジェクトのリスト")]
    public IT_StageObjectList stageObjectList;

    [SerializeField] float blockLength = 15f; // ブロックの長さ
    [SerializeField] int randomStageCount = 5; // ランダムステージの数

    [SerializeField] int StageID = 0; // 現在のステージID




    // private void Start()
    // {
    //     CreateStage();
    // }

    // // 一ブロック pos(0,0,0) scale(8,0.1,15) 縦に並べる
    // private void CreateStage()
    // {
    //     // 開始オブジェクトの生成
    //     if (stageObjectList.startObject != null)
    //     {
    //         Instantiate(stageObjectList.startObject, Vector3.zero, Quaternion.identity);
    //     }

    //     // ステージオブジェクトの生成
    //     for (int i = 0; i < randomStageCount; i++)
    //     {
    //         int randomIndex = Random.Range(0, stageObjectList.stageObjects.Length);
    //         if (stageObjectList.stageObjects[randomIndex] != null)
    //         {
    //             Vector3 position = new Vector3(0, 0, (i + 1) * blockLength); // 縦に並べる
    //             Instantiate(stageObjectList.stageObjects[randomIndex], position, Quaternion.identity);
    //         }
    //     }

    //     // 終了オブジェクトの生成
    //     if (stageObjectList.endObject != null)
    //     {
    //         Vector3 position = new Vector3(0, 0, (randomStageCount + 1) * blockLength); // 縦に並べる
    //         Instantiate(stageObjectList.endObject, position, Quaternion.identity);
    //     }
    // }

    void Start()
    {
        Loading();
    }


    void Loading()
    {
        // プレイヤーの位置に応じてステージを生成する処理

        CSVLoader csvLoader = GetComponent<CSVLoader>();
        List<string[]> csvData = csvLoader.LoadCSV("Stage"); // CSVファイル
        int stageLength = 0; // ステージの長さをカウントする変数

        // 開始オブジェクトの生成
        if (stageObjectList.startObject != null)
        {
            Instantiate(stageObjectList.startObject, Vector3.zero, Quaternion.identity);
        }

        // CSVデータを元にステージを生成する処理
        // 0,1,1,1,1
        // 1,1,1,1
        // これは、最初の一文字はステージIDを表し、次の数字はステージオブジェクトのインデックスを表すと仮定します
        foreach (string[] line in csvData)
        {
            if (line.Length == 0) continue; // 空行をスキップ
            int currentStageID;
            if (!int.TryParse(line[0], out currentStageID)) continue; // ステージIDのパースに失敗した場合はスキップ
            if (currentStageID != StageID) continue; // 現在のステージIDと一致しない場合はスキップ

            for (int i = 1; i < line.Length; i++)
            {
                int objectIndex;
                if (!int.TryParse(line[i], out objectIndex)) continue; // オブジェクトインデックスのパースに失敗した場合はスキップ
                if (objectIndex < 0 || objectIndex >= stageObjectList.stageObjects.Length) continue; // インデックスが範囲外の場合はスキップ

                GameObject stageObject = stageObjectList.stageObjects[objectIndex];
                if (stageObject != null)
                {
                    Vector3 position = new Vector3(0, 0, (stageLength + 1) * blockLength); // 縦に並べる
                    Instantiate(stageObject, position, Quaternion.identity);
                }
                stageLength++; // ステージの長さをカウント
            }
        }


        // 終了オブジェクトの生成
        if (stageObjectList.endObject != null)
        {
            Vector3 position = new Vector3(0, 0, (stageLength + 1) * blockLength); // 縦に並べる
            Instantiate(stageObjectList.endObject, position, Quaternion.identity);
        }
    }



}