using System.Collections.Generic;
using common;
using UnityEngine;

[RequireComponent(typeof(CSVLoader))]
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("ステージオブジェクトのデータベース")]
    [SerializeField] private StageObjectDatabase stageObjectDatabase;

    [Header("使用するステージID")]
    [SerializeField] int StageID = 1; // 現在のステージID
    float blockLength = 5f; // ブロックの長さ
    int currentMas = 0; // 現在のマス
    
    [Header("ステージオブジェクトの親")]
    [SerializeField] Transform stageParent; // ステージオブジェクトの親
    [SerializeField] Transform groundParent; // 地面オブジェクトの親

    [Header("オブジェクトの位置調整")]
    [SerializeField] float objectOffset = 1f; // オブジェクトオフセット(めり込まないように)

#if UNITY_EDITOR
    public bool debugMode = false; // デバッグモードのフラグ
#endif

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

        stageObjectDatabase.Init();
    }
    
    void Start()
    {
        if (debugMode) return; // デバッグモードの場合はステージ生成をスキップ
        Loading();
    }

    void Loading(int sid = -99)
    {
        // プレイヤーの位置に応じてステージを生成する処理
        CSVLoader csvLoader = GetComponent<CSVLoader>();
        List<string[]> csvData = csvLoader.LoadCSV("Stage"); // CSVファイル
        List<StageCellData> stageCells = CSVLoader.Parse(csvData);

        if (sid != -99)
        {
            StageID = sid;
        }

        // // Debug stageCellの内容を確認
        // foreach (var cell in stageCells)
        // {
        //     Debug.Log($"stageId: {cell.stageId}, lane: {cell.lane}, z: {cell.z}, objectId: {cell.objectId}, amount: {cell.amount}");
        // }

        // CSVデータに基づいてステージオブジェクトを生成
        foreach (StageCellData cell in stageCells)
        {
            if (cell.stageId != StageID) continue;

            Vector3 offset = GetLaneOffset(cell.lane);
            Vector3 position = offset + new Vector3(0, 0, cell.z * blockLength);

            var data = stageObjectDatabase.Get(cell.objectId);
            if (data == null && cell.objectId != 0) continue;

            if (cell.objectId == 99)
            {
                // ゴールなので床と壁を生成せず 床壁があるところにこれを生成する
                Instantiate(data.prefab, position, data.prefab.transform.rotation, stageParent);

                continue;
            }


            var obj = Instantiate(data.prefab, position + GetObjectOffset(cell.lane) + data.prefab.transform.position, data.prefab.transform.rotation, stageParent);

            if (obj.TryGetComponent(out StageObjectItem item))
            {
                item.Init(data, cell.amount, cell.z + 1);
            }

            // 穴でなければ生成
            if (data.type == StageObjectType.Hole)
            {
                continue;
            }

            // cellに合わせて床と壁も生成
            if (cell.lane == 0) // 床
            {
                Instantiate(stageObjectDatabase.floorPrefab, position, Quaternion.identity, groundParent);
            }
            else // 壁
            {
                Instantiate(stageObjectDatabase.wallPrefab, position, Quaternion.Euler(GetLaneRotation(cell.lane)), groundParent);
            }
        }
    }


    Vector3 GetLaneOffset(int lane)
    {
        return lane switch
        {
            0 => new Vector3(0, 0, 0),     // 床
            1 => new Vector3(-4, 1, 0),    // 左壁
            2 => new Vector3(4, 1, 0),     // 右壁
            _ => Vector3.zero
        };
    }

    Vector3 GetLaneRotation(int lane)
    {
        return lane switch
        {
            0 => new Vector3(0, 0, 0),     // 床
            1 => new Vector3(0, 0,60),    // 左壁
            2 => new Vector3(0, 0, -60),     // 右壁
            _ => Vector3.zero
        };
    }
    

    Vector3 GetObjectOffset(int lane)
    {
        return lane switch
        {
            0 => new Vector3(0, objectOffset, 0),     // 床
            1 => new Vector3(objectOffset, objectOffset, 0),    // 左壁
            2 => new Vector3(-objectOffset, objectOffset, 0),     // 右壁
            _ => Vector3.zero
        };
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
        currentMas = 0; // 現在のマスをリセット
    }

    [ContextMenu("CreateStage")]
    public void CreateStage()
    {
        Loading();
    }

    public void CreateStage(int id)
    {
        RemoveStage();
        Loading(id);
    }

    #endregion


}
