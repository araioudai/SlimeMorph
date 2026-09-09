using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class AdmobReward : MonoBehaviour
{
    //読み込んだリワード広告を保持する変数
    private RewardedAd rewardedAd;

    //獲得した報酬数を画面に表示するUIテキスト
    [SerializeField] private Text rewardText;
    //獲得報酬のカウント用変数
    private int rewardCount = 0;

    /// <summary>
    /// リワード広告をロード（読み込み）する処理
    /// </summary>
    public void LoadRewardAd()
    {
        string adUnitId;

        //実行環境に応じて広告IDを分岐設定
#if UNITY_ANDROID
        adUnitId = "ca-app-pub-3940256099942544/5224354917"; //Android用テストID
#elif UNITY_IPHONE
        adUnitId = "ca-app-pub-3940256099942544/1712485313"; //iOS用テストID
#else
        adUnitId = "unexpected_platform";
#endif

        //リワード広告の読み込みリクエストを発行
        RewardedAd.Load(adUnitId, new AdRequest(),
            (RewardedAd ad, LoadAdError loadError) =>
            {
                //エラー等で読み込みに失敗した場合
                if (loadError != null)
                {
                    Debug.LogError("広告の読み込みに失敗しました: " + loadError);
                    return;
                }
                else if (ad == null)
                {
                    return;
                }

                //イベントの登録
                //ユーザーが広告を閉じたら、次回用に新しい広告を自動再読み込み
                ad.OnAdFullScreenContentClosed += () => {
                    LoadRewardAd();
                };

                //広告の全画面表示に失敗したら再読み込みを試みる
                ad.OnAdFullScreenContentFailed += (AdError error) => {
                    LoadRewardAd();
                };

                //すでに古い広告オブジェクトが残っている場合はメモリ解放
                if (rewardedAd != null)
                {
                    rewardedAd.Destroy();
                }

                //読み込みが完了した広告を保持
                rewardedAd = ad;
            });
    }

    /// <summary>
    /// 読み込んだリワード広告を表示する処理
    /// </summary>
    public void ShowRewardAd()
    {
        //広告が存在し、かつ再生可能な状態かチェック
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            //広告を表示
            rewardedAd.Show((Reward reward) =>
            {
                HandleUserEarnedReward(reward);
            });
        }
        else
        {
            //まだ読み込みが完了していない場合の警告ログ
            Debug.Log("リワード広告の読み込みが完了していません。");
        }
    }

    /// <summary>
    /// ユーザーが広告を最後まで視聴し、報酬を獲得した際の処理
    /// </summary>
    public void HandleUserEarnedReward(Reward args)
    {
        //報酬カウントを+1して、UIテキストを更新
        if(rewardText != null)
        {
            rewardText.text = (++rewardCount).ToString();
        }

        //AdMob側で設定した報酬の「種類」と「数量」を取得
        string type = args.Type;
        double amount = args.Amount;

        Debug.Log("報酬獲得: " + amount.ToString() + " " + type);
    }
}