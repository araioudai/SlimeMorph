using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlimeMorph.UI
{
    public class CoinDisplay : CurrencyDisplay
    {
        #region シングルトン
        public static CoinDisplay Instance { get; private set; }

        #endregion

        #region Unityイベント関数
        private void Awake()
        {
            //シングルトン管理
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); //既にInstanceがあれば自分を破棄
                return;
            }
            Instance = this;
        }

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

        #region 外部呼出し関数
        /// <summary>
        /// PlayerPrefsに保存されている最新のコイン数を取得してUIを更新する
        /// </summary>
        public void RefreshDisplay()
        {
            int currentCoins = PlayerPrefs.GetInt("UserCoin", 0);
            UpdateDisplay(currentCoins.ToString());
        }

        #endregion
    }
}