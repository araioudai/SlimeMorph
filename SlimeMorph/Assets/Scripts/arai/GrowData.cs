using UnityEngine;

[CreateAssetMenu(fileName = "NewGrowData", menuName = "ScriptableObjects/GrowData")]
public class GrowData : ScriptableObject
{
    [Header("育成番号")]
    [SerializeField] private int growIndex;
    [Header("育成スキル用キー")]
    [SerializeField] private string growKey;
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
    [Header("各育成最大数")]
    [SerializeField] private int growMax = 11;
    [Header("育成必要コイン 0は解放時")]
    [SerializeField] private int[] growCoins;

    //外部から取得するためのプロパティ
    public int GrowIndex => growIndex;

    public string GrowKey => growKey;
    
    public string GrowTitleEn => growTitleEn;
    public string GrowTitleJa => growTitleJa;

    public string GrowExplanationEn => growExplanationEn;
    public string GrowExplanationJa => growExplanationJa;

    public Sprite GrowIcon => growIcon;

    public int[] GrowCoins => growCoins;

#if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更されたときに自動で呼びだし
    /// </summary>
    private void OnValidate()
    {
        //0以下にならないように
        if (growMax < 1) growMax = 1;

        //配列が空、またはサイズがgrowMaxとズレている場合に自動リサイズ
        if (growCoins == null || growCoins.Length != growMax)
        {
            System.Array.Resize(ref growCoins, growMax);
        }
    }
#endif
}