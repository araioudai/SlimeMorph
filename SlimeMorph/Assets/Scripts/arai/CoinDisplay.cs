using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlimeMorph.UI
{
    public class CoinDisplay : CurrencyDisplay
    {
        #region Unityイベント関数
        protected override void Start()
        {
            base.Start();

            //初期表示（ローカルに保存されている前回のコイン数を表示、無ければ0）
            UpdateDisplay(PlayerPrefs.GetInt("UserCoin", 0).ToString());

            //サーバーから最新のコイン数を非同期で取得
            if (OnLineManager.Instance != null)
            {
                OnLineManager.Instance.LoadPlayer((success, playerData) =>
                {
                    if (success && playerData != null)
                    {
                        //サーバーから無事に取得できたら、コイン数を文字(string)に変換して表示更新
                        UpdateDisplay(playerData.coin.ToString());
                    }
                    else
                    {
                        Debug.LogWarning("サーバーからのコインデータ取得に失敗しました。");
                    }
                });
            }
        }

        #endregion
    }
}