using UnityEngine;

namespace SlimeMorph.UI
{
    public class NameDisplay : CurrencyDisplay
    {
        #region Unityイベント関数
        protected override void Start()
        {
            base.Start();

            //初期表示（現在のユーザ名を取得して表示）
            UpdateDisplay(GetSavedNameAmount());
        }

        #endregion

        #region 名前表示処理
        /// <summary>
        /// データ取得メソッド
        /// </summary>
        /// <returns>現在のユーザ名</returns>
        private string GetSavedNameAmount()
        {
            //サーバーから取得しておいたUserNameを表示
            return PlayerPrefs.GetString("UserName", "abc");
        }

        #endregion
    }
}
