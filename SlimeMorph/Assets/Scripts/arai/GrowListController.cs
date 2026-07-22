using System;
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

    [Header("ロード画面")]
    [SerializeField] private GameObject loadingPanel;

    //生成したスロットを管理するためのリスト
    private List<GrowItemSlot> spawnedSlots = new List<GrowItemSlot>();

    //PlayerPrefsで使用する保存用のキー名
    private const string SelectedGrowKey = "SavedSelectedGrowIndex";

    #endregion

    #region Unityイベント関数

    private void Awake()
    {
        //開始時はロードパネルは非表示
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    /// <summary>
    /// スキンパネルが非表示になった瞬間に安全にコルーチンを止める
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
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

                //スキルの「現在のレベル」をロードする
                string levelKey = $"GrowLevel_{data.GrowKey}";
                int currentLevel = PlayerPrefs.GetInt(levelKey, 0);

                slot.Setup(data.GrowIndex, data.GrowKey, data.GrowIcon, data.GrowTitleEn, data.GrowTitleJa, data.GrowExplanationEn, data.GrowExplanationJa, currentLevel, data.GrowCoins);
                slot.OnClicked += OnGrowSelected; //イベント登録
                spawnedSlots.Add(slot);
            }
        }
        else
        {
            //2回目以降は、すでにあるスロットの表示を最新データに更新
            Debug.Log("[GrowList]既存のスロットを再利用、表示を更新");

            //すでに生成済みの場合は、最新のレベルに同期させて再描画
            for (int i = 0; i < spawnedSlots.Count; i++)
            {
                int index = spawnedSlots[i].Index;
                string key = spawnedSlots[i].GrowKey;
                string levelKey = $"GrowLevel_{key}";
                int currentLevel = PlayerPrefs.GetInt(levelKey, 0);

                spawnedSlots[i].UpdateLevel(currentLevel);
            }
        }

        //二重動作防止のために念のため一度止めてから、選択中のスロットへスクロールを開始
        StopAllCoroutines();
    }

    /// <summary>
    /// 強化が選択されたときに実行されるコールバック処理
    /// </summary>
    /// <param name="selectedKey">選択された強化のキー</param>
    private void OnGrowSelected(string selectedKey)
    {
        //データの安全確認
        GrowData data = growDataList.Find(d => d.GrowKey == selectedKey);
        if (data == null) return;

        //ローカルから現在のレベルを取得
        string levelKey = $"GrowLevel_{selectedKey}";
        int currentLevel = PlayerPrefs.GetInt(levelKey, 0);

        //すでにMAXレベルなら何もしない
        if (currentLevel >= data.GrowCoins.Length){ return; }

        //必要コイン数のチェック
        int neededCoin = data.GrowCoins[currentLevel];
        int myCoins = PlayerPrefs.GetInt("UserCoin", 0); //OnLineManagerが保存しているコインキー

        if (myCoins >= neededCoin)
        {
            //通信を開始、ロード画面を表示
            if (loadingPanel != null) { loadingPanel.SetActive(true); }

            //強化後の値を仮計算
            int newCoin = myCoins - neededCoin;
            int newLevel = currentLevel + 1;

            //ローカルに最新状態を保存する
            PlayerPrefs.SetInt("UserCoin", newCoin);
            PlayerPrefs.SetInt(levelKey, newLevel);
            PlayerPrefs.Save();

            //UIを即座にピンポイント再描画
            GrowItemSlot slot = spawnedSlots.Find(s => s.GrowKey == selectedKey);
            if (slot != null)
            {
                slot.UpdateLevel(currentLevel);
            }

            //他のステータス情報もPlayerPrefsから取得
            int sideSpeedLv = PlayerPrefs.GetInt("GrowLevel_sidespeed_lv", 0);
            int defenceLv = PlayerPrefs.GetInt("GrowLevel_defence_lv", 0);
            int shrinkLv = PlayerPrefs.GetInt("GrowLevel_shrink_lv", 0);
            int clearStage = PlayerPrefs.GetInt("ClearStage", 1);
            int stamina = PlayerPrefs.GetInt("Stamina", 5);
            string recoveryTime = PlayerPrefs.GetString("StaminaRecovery", "");

            //サーバーへの同期処理
            OnLineManager.Instance.SavePlayer(
                newCoin,
                sideSpeedLv,
                defenceLv,
                shrinkLv,
                clearStage,
                stamina,
                recoveryTime,
                (bool isSuccess) =>
                {
                    if(loadingPanel != null) { loadingPanel.SetActive(false); }

                    if (isSuccess)
                    {
                        //成功時：UIを更新
                        GrowItemSlot slot = spawnedSlots.Find(s => s.GrowKey == selectedKey);
                        if (slot != null)
                        {
                            slot.UpdateLevel(newLevel);
                        }

                        //コイン表示更新
                        if (SlimeMorph.UI.CoinDisplay.Instance != null)
                        {
                            SlimeMorph.UI.CoinDisplay.Instance.RefreshDisplay();
                        }

                        Debug.Log($"[強化・保存成功] Key: {selectedKey} -> 新レベル: {newLevel} (残りコイン: {newCoin})");
                    }
                    else
                    {
                        //失敗時：ローカルデータをもとに戻す
                        PlayerPrefs.SetInt("UserCoin", myCoins);
                        PlayerPrefs.SetInt(levelKey, currentLevel);
                        PlayerPrefs.Save();

                        Debug.LogError("サーバーへの保存に失敗したため、ローカルデータを元に戻しました。");
                    }
                });
        }
        else
        {
            //コインが足りない時のエラー演出（警告SEなど）
            Debug.LogWarning("コインが不足しています！");
        }
    }

    #endregion













    /// <summary>
    /// 【デバッグ用】すべての強化状況をリセットし、テスト用コインを付与する
    /// </summary>
    public void Debug_ResetAndGrantCoins()
    {
        //テスト用にコインを多めに付与
        int debugCoins = 10000;
        PlayerPrefs.SetInt("UserCoin", debugCoins);

        //リスト内にあるすべての強化レベルを「0」にリセットしてセーブ
        for (int i = 0; i < growDataList.Count; i++)
        {
            int index = growDataList[i].GrowIndex;
            PlayerPrefs.SetInt($"SavedGrowLevel_{index}", 0);
        }
        PlayerPrefs.Save();

        //現在画面に並んでいるスロットのUIを即座に再描画
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            spawnedSlots[i].UpdateLevel(0); //レベル0（未解放）で再描画
        }

        //コイン表示UIも即座に更新
        // HeaderUI.Instance.UpdateCoinDisplay();

        Debug.Log($"<color=yellow>【デバッグ】データをリセットし、コインを {debugCoins:N0} 付与しました！</color>");
    }
}
