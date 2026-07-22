using UnityEngine;

public class StageCoin : StageObjectItem
{
    [SerializeField] float coinSpeed = 100f; // コインの回転速度\
    [SerializeField] AudioClip coinSound; // コイン取得音
    bool isGetCoin = false; // コインを取得したかどうかのフラグ


    void Update()
    {
        if (isStop) return; // isStopがtrueの場合は処理をスキップ

        // コインを回転させる
        transform.eulerAngles += new Vector3(0, coinSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isGetCoin)
        {
            isGetCoin = true; // コインを取得したことを記録

            IT_GameManager.Instance.GetCoin((int)Amount); // コインを加算


            // コイン取得音を再生
            if (coinSound != null)
            {
                SoundManager.Instance.PlaySE(coinSound);
            }
            Destroy(gameObject); // コインを破壊
        }
    }

}
