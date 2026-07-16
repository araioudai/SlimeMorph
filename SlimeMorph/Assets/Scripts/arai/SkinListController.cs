using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SkinListController : MonoBehaviour
{
    #region private変数
    [Header("スキン選択関係")]
    [SerializeField] private GameObject skinItemPrefab;
    [SerializeField] private Transform contentTransform;

    [Header("スクロールビューの本体")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("3Dモデルを管理しているSkinManager")]
    [SerializeField] private SkinManager skinManager;

    [Header("スキンデータリスト")]
    [SerializeField] private List<SkinData> skinDataList = new List<SkinData>();

    //生成したスロットを管理するためのリスト
    private List<SkinItemSlot> spawnedSlots = new List<SkinItemSlot>();

    //PlayerPrefsで使用する保存用のキー名
    private const string SelectedSkinKey = "SavedSelectedSkinIndex";

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

    #region スキンリストの生成と選択制御

    /// <summary>
    /// スキン一覧の初期化（初回のみ生成、2回目以降は表示更新とスクロール）
    /// </summary>
    public void InitializeSkinList()
    {
        //保存されているスキンのインデックスをロード
        int savedIndex = PlayerPrefs.GetInt(SelectedSkinKey, 0);

        //まだスロットが生成されていない場合のみ生成
        if (spawnedSlots.Count == 0)
        {
            Debug.Log("[SkinList]スロットを新規生成");

            for (int i = 0; i < skinDataList.Count; i++)
            {
                SkinData data = skinDataList[i];
                if (data == null) continue;

                GameObject newItem = Instantiate(skinItemPrefab, contentTransform);
                SkinItemSlot slot = newItem.GetComponent<SkinItemSlot>();

                slot.Setup(data.SkinIndex, data.SkinIcon);
                slot.OnClicked += OnSkinSelected; //イベント登録
                spawnedSlots.Add(slot);

                //ロードされた値と一致するかで選択状態を初期化
                slot.SetSelectState(data.SkinIndex == savedIndex);
            }
        }
        else
        {
            //2回目以降は、すでにあるスロットの表示（枠線）を最新データに更新
            Debug.Log("[SkinList]既存のスロットを再利用、表示を更新");

            foreach (SkinItemSlot slot in spawnedSlots)
            {
                slot.SetSelectState(slot.Index == savedIndex);
            }
        }

        //3Dモデル側の表示も現在のセーブデータに合わせる
        if (skinManager != null)
        {
            skinManager.ChangeSkin(savedIndex);
        }

        //二重動作防止のために念のため一度止めてから、選択中のスロットへスクロールを開始
        StopAllCoroutines();

        StartCoroutine(ScrollToSelectedSlot(savedIndex));
    }

    /// <summary>
    /// 指定されたインデックスのスロットまで自動スクロール
    /// </summary>
    private IEnumerator ScrollToSelectedSlot(int targetIndex)
    {
        //ScrollRectがない場合や、インデックスが異常な場合は処理しない
        if (scrollRect == null || targetIndex < 0 || targetIndex >= spawnedSlots.Count) yield break;

        yield return null;

        //スロットのRectTransformを取得
        RectTransform targetRect = spawnedSlots[targetIndex].GetComponent<RectTransform>();

        //ビューポートから見た、ターゲットの相対的な位置を計算
        Vector2 targetPosInViewport = scrollRect.viewport.InverseTransformPoint(targetRect.position);

        //現在のスクロールの座標を取得
        Vector2 newAnchoredPosition = scrollRect.content.anchoredPosition;

        //縦スクロールの場合、ターゲットが収まるようにY座標を調整
        if (scrollRect.vertical)
        {
            newAnchoredPosition.y -= targetPosInViewport.y;
        }

        //計算した位置を適用してスクロール
        scrollRect.content.anchoredPosition = newAnchoredPosition;
    }

    /// <summary>
    /// スキンが選択されたときに実行されるコールバック処理
    /// </summary>
    /// <param name="index">選択されたスキンのインデックス番号</param>
    private void OnSkinSelected(int selectedIndex)
    {
        Debug.Log($"[SkinList] スキン {selectedIndex} の選択を検知。枠の表示を更新");

        //選択されたインデックスをPlayerPrefsに保存
        PlayerPrefs.SetInt(SelectedSkinKey, selectedIndex);
        PlayerPrefs.Save();

        //リストにいるすべてのスロットに選択されたか
        foreach (SkinItemSlot slot in spawnedSlots)
        {
            //自分のインデックスが、今選ばれたインデックスと同じなら true、違えば false
            bool isCurrentSelected = (slot.Index == selectedIndex);

            //スロットに通知して、見た目を変えさせる
            slot.SetSelectState(isCurrentSelected);
        }

        //3Dモデル側の表示も選択されたスキンに合わせる
        if (skinManager != null)
        {
            skinManager.ChangeSkin(selectedIndex);
        }
    }

    #endregion
}