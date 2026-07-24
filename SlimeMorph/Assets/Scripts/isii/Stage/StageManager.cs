using System.Collections.Generic;
using common;
using UnityEngine;

[RequireComponent(typeof(CSVLoader))]
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("ステージオブジェクトのデータベース")]
    [SerializeField] private StageObjectDatabase stageObjectDatabase;
    public float GetStageObjectParam(int objectId)
    {
        var data = stageObjectDatabase.Get(objectId);
        return data != null ? data.param : 0f;
    }

    [Header("使用するステージID")]
    [SerializeField] int stageID = 1; // 現在のステージID
    float blockLength = 5f; // ブロックの長さ
    int currentMas = 0; // 現在のマス
    [SerializeField] int maxStageID = 10; // 最大ステージID
    
    [Header("ステージオブジェクトの親")]
    [SerializeField] Transform stageParent; // ステージオブジェクトの親
    [SerializeField] Transform groundParent; // 地面オブジェクトの親

    [Header("オブジェクトの位置調整")]
    [SerializeField] float objectOffset = 1f; // オブジェクトオフセット(めり込まないように)

    [Header("季節ごとのマテリアル")]
    [SerializeField] private Material springMaterial;
    [SerializeField] private Material summerMaterial;
    [SerializeField] private Material autumnMaterial;
    [SerializeField] private Material winterMaterialAdd;

    private int stageNumber;



    public int GetStageID()
    {
        return stageNumber;
    }



    public bool debugMode = false; // デバッグモードのフラグ

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
        // if (debugMode) return; // デバッグモードの場合はステージ生成をスキップ

        if (debugMode)
        {
            Loading();
            return;
        }
        int clearStage = PlayerPrefs.GetInt("ClearStage", 0);

        Debug.Log(clearStage);

        stageID = clearStage; // クリアしたステージに応じてステージIDを設定

        if (stageID == 0)
        {
            stageID += 1;
        }

        stageNumber = stageID + 1;

        Debug.Log(stageID);

        if (stageID > maxStageID)
        {
            // 1-10の範囲でランダムにステージIDを設定
            stageID = Random.Range(1, maxStageID + 1);
        }

        Debug.Log(stageID);

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
            stageID = sid;
        }

        // // Debug stageCellの内容を確認
        // foreach (var cell in stageCells)
        // {
        //     Debug.Log($"stageId: {cell.stageId}, lane: {cell.lane}, z: {cell.z}, objectId: {cell.objectId}, amount: {cell.amount}");
        // }

        // CSVデータに基づいてステージオブジェクトを生成
        foreach (StageCellData cell in stageCells)
        {
            if (cell.stageId != stageID) continue;

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

            if (data.prefab != null)
            {
                var obj = Instantiate(data.prefab, position + GetObjectOffset(cell.lane) + data.prefab.transform.position, data.prefab.transform.rotation, stageParent);
                if (obj.TryGetComponent(out StageObjectItem item))
                {
                    item.Init(data, cell.amount, cell.z + 1);
                    IT_GameManager.Instance.RegisterStageObject(item);
                }
            }



            // 穴でなければ生成
            if (data.type == StageObjectType.Hole)
            {
                Instantiate(stageObjectDatabase.holePrefab, position, Quaternion.Euler(GetLaneRotation(cell.lane)), stageParent);
                continue;
            }

            // cellに合わせて床と壁も生成
            // if (cell.lane == 0) // 床
            // {
            //     var floor = Instantiate(stageObjectDatabase.floorPrefab, position, Quaternion.identity, groundParent);
            //     // 季節に応じてマテリアルを変更
            //     var renderer = floor.GetComponent<Renderer>();
            //     if (renderer != null)
            //     {
            //         switch (cell.season)
            //         {
            //             case StageSeason.Spring:
            //                 renderer.material = springMaterial;
            //                 break;
            //             case StageSeason.Summer:
            //                 renderer.material = summerMaterial;
            //                 break;
            //             case StageSeason.Autumn:
            //                 renderer.material = autumnMaterial;
            //                 break;
            //             case StageSeason.Winter:
            //                 renderer.materials[1] = winterMaterialAdd; // 2つ目のマテリアルを変更
            //                 break;
            //         }
            //     }
            // }
            // else // 壁
            // {
            //     Instantiate(stageObjectDatabase.floorPrefab, position, Quaternion.Euler(GetLaneRotation(cell.lane)), groundParent);
            // }

            {
                var floor = Instantiate(stageObjectDatabase.floorPrefab, position, Quaternion.Euler(GetLaneRotation(cell.lane)), groundParent);
                // 季節に応じてマテリアルを変更
                if (floor.TryGetComponent<MeshRenderer>(out var renderer))
                {
                    switch (cell.season)
                    {
                        case StageSeason.Spring:
                            renderer.material = springMaterial;
                            break;
                        case StageSeason.Summer:
                            renderer.material = summerMaterial;
                            break;
                        case StageSeason.Autumn:
                            renderer.material = autumnMaterial;
                            break;
                        case StageSeason.Winter:
                            // マテリアル 追加
                            var materials = renderer.materials;
                            if (materials.Length > 1)
                            {
                                materials[1] = winterMaterialAdd; // 2つ目のマテリアルを変更
                                renderer.materials = materials;
                            }
                            else
                            {
                                Debug.LogWarning($"マテリアルが2つ以上ない: {floor.name}", floor);
                            }
                            break;
                    }
                    // Debug.Log($"cell.season: {cell.season}, cell.lane: {cell.lane}, cell.z: {cell.z}, position: {position}", this);
                }
                else
                {
                    Debug.LogWarning($"Rendererが見つかりません: {floor.name}", floor);
                    Debug.LogWarning($"cell.season: {cell.season}, cell.lane: {cell.lane}, cell.z: {cell.z}, position: {position}", this);
                }
            }


        }
    }

    #region Offsets
    Vector3 GetLaneOffset(int lane)
    {
        return lane switch
        {
            0 => new Vector3(0, 0, 0),     // 床
            1 => new Vector3(-4.3f, 1.1f, 0),    // 左壁
            2 => new Vector3(4.3f, 1.1f, 0),     // 右壁
            _ => Vector3.zero
        };
    }

    Vector3 GetLaneRotation(int lane)
    {
        return lane switch
        {
            0 => new Vector3(0, 0, 0),     // 床
            1 => new Vector3(0, 0, -30),    // 左壁
            2 => new Vector3(0, 0, 30),     // 右壁
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
    #endregion
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
