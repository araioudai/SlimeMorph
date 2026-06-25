using System;
using UnityEngine;
using UnityEngine.UI;

public class SkinItemSlot : MonoBehaviour
{
    #region private変数
    [Header("ボタン")]
    [SerializeField] private Button button;
    [Header("選択中の枠")]
    [SerializeField] private GameObject selectionFrame;

    //クリックされたことを外部に知らせるイベント（Action）
    public event Action<int> OnClicked;

    //インデックスを外部から読めるように
    public int Index { get; private set; }

    #endregion

    #region 関数
    /// <summary>
    /// スロットの初期設定、クリックイベントを紐付け
    /// </summary>
    /// <param name="index">スロットに割り当てるインデックス番号</param>
    public void Setup(int index)
    {
        //インデックスを保持
        Index = index;

        //ボタンのイベントが重複して登録されるのを防ぐため、一度クリアする
        button.onClick.RemoveAllListeners();

        //ボタンが押されたら、イベントを発火してコントローラーに通知
        button.onClick.AddListener(() => OnClicked?.Invoke(Index));
    }

    /// <summary>
    /// 選択状態に合わせて、枠の表示を切り替える
    /// </summary>
    public void SetSelectState(bool isSelected)
    {
        if (selectionFrame != null)
        {
            selectionFrame.SetActive(isSelected);
        }
    }

    #endregion
}