using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StagePlayerPrefabs", menuName = "ScriptableObjects/StagePlayerPrefabs", order = 1)]
public class StagePlayerPrefabs : ScriptableObject
{
    [SerializeField]
    public List<GameObject> prefabs = new();
}
