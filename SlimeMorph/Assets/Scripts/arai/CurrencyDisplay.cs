using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlimeMorph.UI
{
    public class CurrencyDisplay : MonoBehaviour
    {
        #region protected変数
        [Header("UIコンポーネント")]
        [SerializeField] protected TextMeshProUGUI currencyAmountText;
        [SerializeField] protected Image currencyIcon;

        [Header("アイコン設定")]
        [SerializeField] protected Sprite coinIconSprite;

        #endregion

        #region Unityイベント関数
        protected virtual void Start()
        {
            if (currencyIcon != null && coinIconSprite != null)
            {
                currencyIcon.sprite = coinIconSprite;
            }
            else
            {
                //カラーを新しく生成して代入
                Color color = currencyIcon.color;
                color.a = 0f;
                currencyIcon.color = color;
            }
        }

        #endregion

        #region コイン表示処理

        /// <summary>
        /// 表示を更新するメソッド
        /// </summary>
        /// <param name="currentAmount">表示するもの</param>
        protected void UpdateDisplay(string currentAmount)
        {
            if (currencyAmountText != null)
            {
                currencyAmountText.text = currentAmount;
            }
        }

        #endregion
    }
}