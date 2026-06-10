using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageObjectDatabase", menuName = "ScriptableObjects/StageObjectDatabase", order = 2)]
public class StageObjectDatabase : ScriptableObject
{
    [Header("ステージオブジェクトのデータリスト")]
    [SerializeField] private List<StageObjectData> dataList;

    [Header("床・壁のデータリスト")]
    [SerializeField] public GameObject floorPrefab;
    [SerializeField] public GameObject wallPrefab;

    private Dictionary<int, StageObjectData> dataDict;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        dataDict = new Dictionary<int, StageObjectData>();

        foreach (var data in dataList)
        {
            dataDict[data.id] = data;
        }
    }

    /// <summary>
    /// IDから取得
    /// </summary>
    public StageObjectData Get(int id)
    {
        if (dataDict.TryGetValue(id, out var data))
        {
            return data;
        }

        Debug.LogWarning($"IDが見つからない: {id}", this);
        return null;
    }}
