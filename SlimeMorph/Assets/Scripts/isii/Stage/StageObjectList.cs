using UnityEngine;

[CreateAssetMenu(fileName = "StageObjectList", menuName = "ScriptableObjects/StageObjectList", order = 1)]
public class StageObjectList : ScriptableObject
{
    [Header("ステージオブジェクトのリスト")]
    public GameObject[] stageObjects;

    [Header("開始時のオブジェクト")]
    public GameObject startObject;

    [Header("終了時のオブジェクト")]
    public GameObject endObject;

}
