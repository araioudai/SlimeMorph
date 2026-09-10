using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject loadingPanel;                              //ロード表示パネル

    //生成したスロットを管理するためのリスト
    private List<GrowItemSlot> spawnedSlots = new List<GrowItemSlot>();

    //PlayerPrefsで使用する保存用のキー名
    private const string SelectedGrowKey = "SavedSelectedGrowIndex";

    //通信失敗時のリトライおよびロールバック用、直前に操作したデータを一時保持変数
    private string lastSelectedKey; //直前に選択した強化キー

    #endregion

    #region Unityイベント関数

    private void Awake()
    {
        //開始時はロードパネルとエラーパネルは非表示
        if (loadingPanel != null) { loadingPanel.SetActive(false); }
    }

    /// <summary>
    /// 非アクティブになった時にイベントを解除、コルーチンを止める
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();

        //パネルを非表示
        if (loadingPanel != null) { loadingPanel.SetActive(false); }
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

        //コインが足りているかどうか
        if (myCoins >= neededCoin)
        {
            //通信失敗時ように、強化前の数値を記憶
            lastSelectedKey = selectedKey;

            //通信を開始、ロード画面を表示
            if (loadingPanel != null) { loadingPanel.SetActive(true); }

            //強化後の値を仮計算
            int newCoin = myCoins - neededCoin;
            int newLevel = currentLevel + 1;

            //サーバーへ保存通信を開始
            SendSaveRequest(selectedKey, newCoin, newLevel);
        }
        else
        {
            //コインが足りない時のエラー演出（警告SEなど）
            Debug.LogWarning("コインが不足しています！");
        }
    }

    /// <summary>
    /// サーバーへ最新ステータスを同期送信する処理
    /// </summary>
    private void SendSaveRequest(string selectedKey, int newCoin, int newLevel)
    {
        //送信するパラメータのうち、今回強化するキーだけ newLevel を適用する
        int sideSpeedLv = (selectedKey == "sidespeed_lv") ? newLevel : PlayerPrefs.GetInt("GrowLevel_sidespeed_lv", 0);
        int defenceLv = (selectedKey == "defence_lv") ? newLevel : PlayerPrefs.GetInt("GrowLevel_defence_lv", 0);
        int shrinkLv = (selectedKey == "shrink_lv") ? newLevel : PlayerPrefs.GetInt("GrowLevel_shrink_lv", 0);
        //ローカルから取得
        int clearStage = PlayerPrefs.GetInt("ClearStage", 0);
        int stamina = PlayerPrefs.GetInt("Stamina", 5);
        string recoveryTime = PlayerPrefs.GetString("StaminaRecovery", "");

        //PlayerBackend.Currentを介してAPIリクエスト実行
        PlayerBackend.Current.SavePlayer(
            newCoin, sideSpeedLv, defenceLv, shrinkLv, clearStage, stamina, recoveryTime,
            (bool isSuccess) =>
            {
                //通信完了のためローディング非表示
                if (loadingPanel != null) { loadingPanel.SetActive(false); }

                if (isSuccess)
                {
                    //通信が成功した瞬間にPlayerPrefsを更新とセーブ
                    string levelKey = $"GrowLevel_{selectedKey}";
                    PlayerPrefs.SetInt("UserCoin", newCoin);
                    PlayerPrefs.SetInt(levelKey, newLevel);
                    LocalCommon.SaveLocalTimeStamp();

                    //UIの更新値を currentLevel（旧Lv）から newLevel（新Lv）へ修正
                    GrowItemSlot slot = spawnedSlots.Find(s => s.GrowKey == selectedKey);
                    if (slot != null)
                    {
                        slot.UpdateLevel(newLevel);
                    }

                    //【通信成功】確定処理
                    //ヘッダー等のコイン表示UIを同期更新
                    if (SlimeMorph.UI.CoinDisplay.Instance != null)
                    {
                        SlimeMorph.UI.CoinDisplay.Instance.RefreshDisplay();
                    }

                    Debug.Log($"[強化通信成功] Key: {selectedKey} -> 新Lv: {newLevel} (残りコイン: {newCoin})");
                }
                else
                {
                    //【通信失敗】エラーダイアログを表示（リトライ／ロールバック）
                    ErrorManager.Instance.ShowError(
                        onRetry: () => OnClickRetry(),
                        onClose: () => OnClickErrorClose()
                    );
                }
            }
        );
    }

    #endregion

    #region エラーハンドリング・ロールバック処理

    /// <summary>
    /// エラーダイアログでリトライが押された時の処理
    /// </summary>
    private void OnClickRetry()
    {
        if (!string.IsNullOrEmpty(lastSelectedKey))
        {
            Debug.Log("[リトライ] 再度強化通信を行います。");
            OnGrowSelected(lastSelectedKey);
        }
    }

    /// <summary>
    /// エラーダイアログで閉じるが押された時の処理
    /// </summary>
    private void OnClickErrorClose()
    {
        //待機画面に戻る
        if (TitleManager.Instance != null)
        {
            TitleManager.Instance.ReturnToStandPanel();
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
