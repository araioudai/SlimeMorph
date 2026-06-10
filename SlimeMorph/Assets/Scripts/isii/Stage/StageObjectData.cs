using UnityEngine;
using common;

[CreateAssetMenu(fileName = "StageObjectData", menuName = "ScriptableObjects/StageObjectData", order = 1)]
public class StageObjectData : ScriptableObject
{
    public int id;
    public StageObjectType type;
    public int param;
    public GameObject prefab;
}
