using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BackObjectList", menuName = "ScriptableObjects/BackObjectList", order = 1)]
public class BackObjectList : ScriptableObject
{
    public List<GameObject> backObjects = new();
}
