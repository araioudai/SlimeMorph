using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowListController : MonoBehaviour
{
    #region private変数
    [Header("スキン選択関係")]
    [SerializeField] private GameObject growItemPrefab;
    [SerializeField] private Transform contentTransform;

    [Header("スクロールビューの本体")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("スキンデータリスト")]
    [SerializeField] private List<GrowData> growDataList = new List<GrowData>();

    //生成したスロットを管理するためのリスト
    private List<GrowItemSlot> spawnedSlots = new List<GrowItemSlot>();

    //PlayerPrefsで使用する保存用のキー名
    private const string SelectedGrowKey = "SavedSelectedGrowIndex";

    #endregion

    #region Unityイベント関数

    /// <summary>
    /// スキンパネルが非表示になった瞬間に安全にコルーチンを止める
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    #endregion

    #region 強化リストの生成と選択制御

    /// <summary>
    /// 強化一覧の初期化（初回のみ生成、2回目以降は表示更新）
    /// </summary>
    public void InitializeGrowList()
    {
        //保存されているスキンのインデックスをロード
        int savedIndex = PlayerPrefs.GetInt(SelectedGrowKey, 0);

        //まだスロットが生成されていない場合のみ生成
        if (spawnedSlots.Count == 0)
        {
            Debug.Log("[GrowList]スロットを新規生成");

            for (int i = 0; i < growDataList.Count; i++)
            {
                GrowData data = growDataList[i];
                if (data == null) continue;

                GameObject newItem = Instantiate(growItemPrefab, contentTransform);
                GrowItemSlot slot = newItem.GetComponent<GrowItemSlot>();

                slot.Setup(data.GrowIndex, data.GrowIcon, data.GrowTitleEn, data.GrowTitleJa, data.GrowExplanationEn, data.GrowExplanationJa);
                slot.OnClicked += OnSkinSelected; //イベント登録
                spawnedSlots.Add(slot);
            }
        }
        else
        {
            //2回目以降は、すでにあるスロットの表示を最新データに更新
            Debug.Log("[GrowList]既存のスロットを再利用、表示を更新");
        }

        //二重動作防止のために念のため一度止めてから、選択中のスロットへスクロールを開始
        StopAllCoroutines();
    }

    /// <summary>
    /// スキンが選択されたときに実行されるコールバック処理
    /// </summary>
    /// <param name="index">選択された強化のインデックス番号</param>
    private void OnSkinSelected(int selectedIndex)
    {
        Debug.Log($"[GrowList] 強化 {selectedIndex} の選択を検知。");

        //選択されたインデックスをPlayerPrefsに保存
        PlayerPrefs.SetInt(SelectedGrowKey, selectedIndex);
        PlayerPrefs.Save();

        //リストにいるすべてのスロットに選択されたか
        /*foreach (GrowItemSlot slot in spawnedSlots)
        {
            //自分のインデックスが、今選ばれたインデックスと同じなら true、違えば false
            bool isCurrentSelected = (slot.Index == selectedIndex);
        }*/
    }

    #endregion
}
