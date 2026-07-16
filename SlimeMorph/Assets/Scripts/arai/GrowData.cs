using UnityEngine;

[CreateAssetMenu(fileName = "NewGrowData", menuName = "ScriptableObjects/GrowData")]
public class GrowData : ScriptableObject
{
    [Header("育成番号")]
    [SerializeField] private int growIndex;
    [Header("育成説明（英語タイトル）")]
    [SerializeField] private string growTitleEn;
    [Header("育成説明（日本語タイトル）")]
    [SerializeField] private string growTitleJa;
    [Header("育成説明（英語詳細）")]
    [SerializeField] private string growExplanationEn;
    [Header("育成説明（日本語詳細）")]
    [SerializeField] private string growExplanationJa;
    [Header("育成アイコン")]
    [SerializeField] private Sprite growIcon;

    //外部から取得するためのプロパティ
    public int GrowIndex => growIndex;
    
    public string GrowTitleEn => growTitleEn;
    public string GrowTitleJa => growTitleJa;

    public string GrowExplanationEn => growExplanationEn;
    public string GrowExplanationJa => growExplanationJa;

    public Sprite GrowIcon => growIcon;
}