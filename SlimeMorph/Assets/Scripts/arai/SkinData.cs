using UnityEngine;

[CreateAssetMenu(fileName = "NewSkinData", menuName = "ScriptableObjects/SkinData")]
public class SkinData : ScriptableObject
{
    [Header("スキン番号")]
    [SerializeField] private int skinIndex;
    [Header("スキンの名前")]
    [SerializeField] private string skinName;
    [Header("スキンアイコン")]
    [SerializeField] private Sprite skinIcon;

    //外部から取得するためのプロパティ
    public int SkinIndex => skinIndex;
    public string SkinName => skinName;
    public Sprite SkinIcon => skinIcon;
}