using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System;

public class UIShaderFader : MonoBehaviour
{
    #region private変数
    [SerializeField] private GameObject fadeObject;
    [SerializeField] private Image fadeImage; //画面全体を覆う黒いImage
    private Material fadeMaterial;            //スクリプトから数値を動的に変更するためのマテリアル参照
    private bool isExiting;                   //二重遷移防止フラグ（ボタン連打対策）

    //シェーダー内のプロパティ名（文字列）を、Unity内部で高速処理できる「整数ID」に変換してキャッシュ
    private readonly int radiusID = Shader.PropertyToID("_FadeRadius");
    #endregion

    private void Awake()
    {
        //他のシーンや別のオブジェクトで同じマテリアルを使い回していても、値の変更が干渉しなくなる
        fadeMaterial = fadeImage.material;
    }

    private void OnEnable()
    {
        //シーン開始時や、オブジェクトがアクティブになった瞬間にフラグを初期化
        isExiting = false;
    }

    /// <summary>
    /// 視界が開く演出（フェードイン：中央から円が広がってゲーム画面が見えるようになる）
    /// </summary>
    /// <param name="duration">演出にかける時間（秒）</param>
    /// <param name="onComplete">演出終了時に実行したい処理（コールバック）</param>
    public IEnumerator PlayFadeIn(float duration, Action onComplete = null)
    {
        //フェード用オブジェクト有効化
        fadeObject.SetActive(true);

        //以前実行中だったTweenアニメーションがあれば安全のために強制停止（重複バグの防止）
        fadeMaterial.DOKill();

        //演出開始の直前に、半径を強制的に「0」にして画面全体を完全に真っ黒（隠れた状態）にする
        fadeMaterial.SetFloat(radiusID, 0f);

        //画面が切り替わった直後のパキッとした見た目の違和感を和らげるため、一瞬（0.3秒）だけ待機
        yield return new WaitForSecondsRealtime(0.3f);

        //DOTweenを使い、マテリアルの半径（radiusID）を 0.0 から 1.5（完全開通）まで滑らかに変化させる
        fadeMaterial.DOFloat(1.5f, radiusID, duration)
            .SetEase(Ease.OutCubic) //じわっと減速しながら、心地よいスピード感で開くイージング
            .SetUpdate(true);       //Time.timeScale = 0（ポーズ中）であっても強制的に動作させる設定

        //アニメーションが動いている時間分、コルーチン側も非同期で待機する
        yield return new WaitForSecondsRealtime(duration);

        //フェード用オブジェクト無効化
        fadeObject.SetActive(false);

        //コールバック処理を実行
        onComplete?.Invoke();
    }

    /// <summary>
    /// 視界を閉じる演出（フェードアウト：画面の外側から中央に向かって黒く閉じていく）
    /// </summary>
    /// <param name="duration">演出にかける時間（秒）</param>
    /// <param name="onComplete">演出終了時に実行したい処理（次のシーンへの遷移処理など）</param>
    public IEnumerator PlayFadeOut(float duration, Action onComplete = null)
    {
        //すでにシーン遷移中の場合は、ボタンの連打などによる多重処理を防ぐためにここで処理を抜ける
        if (isExiting) yield break;
        isExiting = true; //遷移開始フラグを立てる

        //フェード用オブジェクト有効化
        fadeObject.SetActive(true);

        fadeMaterial.DOKill();

        //DOTweenを使い、マテリアルの半径（radiusID）を現在の状態から「0.0（完全に閉じる）」まで縮小させる
        fadeMaterial.DOFloat(0f, radiusID, duration)
            .SetEase(Ease.OutQuad) //閉じ終わりに少し加速・減速をつけるスムーズなイージング
            .SetUpdate(true);      //ポーズ中も動作
            

        //アニメーションの時間分、コルーチン側も待機
        yield return new WaitForSecondsRealtime(duration);

        //フェードが完全に終わったらフラグを戻す
        isExiting = false;

        //コールバック処理を実行
        onComplete?.Invoke();
    }
}