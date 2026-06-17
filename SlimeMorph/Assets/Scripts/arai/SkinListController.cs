using UnityEngine;
using UnityEngine.UI;

public class SkinListController : MonoBehaviour
{
    [Header("スキン選択関係")]
    [SerializeField] private GameObject skinItemPrefab;
    [SerializeField] private Transform contentTransform;

    [Header("生成するスキンボタン数")]
    [SerializeField] private int debugSkinCount = 20;

    void Start()
    {
        GenerateSkinList();
    }

    public void GenerateSkinList()
    {
        //既存のリストをクリア
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        //スキンの数だけループして生成
        for (int i = 0; i < debugSkinCount; i++)
        {
            GameObject newItem = Instantiate(skinItemPrefab, contentTransform);

            // ここで、生成したアイテムのコンポーネントを叩いて
            // 「スキン画像」「解放状態」「選択中か」などのデータを渡す処理
            //newItem.GetComponent<SkinItemSlot>().Setup(skinData[i]);
        }
    }
}