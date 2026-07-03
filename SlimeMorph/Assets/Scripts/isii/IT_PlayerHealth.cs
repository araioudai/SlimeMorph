using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;
public class IT_PlayerHealth : StageObjectItem
{
    [SerializeField] private float maxScale = 3; // 最大スケール値
    [SerializeField] private float minScale = 0.2f; // 最小スケール値




    [Header("移動速度")]
    [SerializeField] float speed = 5f;
    public float Speed { get { return speed; } } // 移動速度を外部から取得できるようにするプロパティ
    bool isMorphed = false;
    [Header("時間経過で減るサイズの量")]
    [SerializeField] float timeMorphDown = 1f; // 時間経過で減るサイズの量

    [Header("重力の値")]
    [SerializeField] float gravityValue = 3; // 重力の値
    public float GravityValue { get { return gravityValue; } } // 重力の値を外部から取得できるようにするプロパティ


    [Header("時間経過でサイズを減らすかどうか")]
    [SerializeField] bool isTimeMorphDown = true; // 時間経過でサイズを減らすかどうか

    bool isGoal = false; // ゴールに到達したかどうかを管理するフラグ
    bool isDead = false; // 死亡しているかどうかを管理するフラグ

    BoxCollider collider;
    void Start()
    {
        collider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGoal) return; // ゴールに到達している場合は移動しない
        if (isDead) return; // 死亡している場合は移動しない
        if (isStop) return; // isStopがtrueの場合は処理をスキップ



        // 前方に移動
        transform.Translate(speed * Time.deltaTime * Vector3.forward);
        // カメラもプレイヤーと同じ速度で移動 ただしカメラは回転している為、プレイヤーの位置に合わせてカメラの位置を更新する
        // Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z - cameraDistance);
        Dead();
        // DeadFall();
        MorphDown();
    }

    public void Morph(float morphSize)
    {
        if(isMorphed) return;
        isMorphed = true;
        // プレイヤーのサイズを+-変更
        transform.localScale += new Vector3(morphSize, morphSize, morphSize);
        collider.size += new Vector3(morphSize, morphSize, morphSize); // コライダーのサイズも変更
        if (morphSize < 0)
            gravityValue -= 1;
        else
            gravityValue += 1;
        StartCoroutine(MorphTimer());
        Debug.Log($"Morph: {morphSize}");
    }

    void MorphDown()
    {
        if (!isTimeMorphDown) return;
        // 時間経過でサイズを減らす
        transform.localScale -= new Vector3(timeMorphDown * Time.deltaTime, timeMorphDown * Time.deltaTime, timeMorphDown * Time.deltaTime);
        collider.size -= new Vector3(timeMorphDown * Time.deltaTime, timeMorphDown * Time.deltaTime, timeMorphDown * Time.deltaTime); // コライダーのサイズも変更
        gravityValue -= timeMorphDown * Time.deltaTime;
    }

    IEnumerator MorphTimer()
    {
        yield return new WaitForSeconds(0.01f);
        isMorphed = false;
    }

    void Dead()
    {
        if(transform.localScale.x <= 0.01f)
        {
            // ゲームオーバー シーン再ロード
            Die(); // プレイヤーの死亡処理を呼び出す
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out StageCoin coin))
        {
            IT_GameManager.Instance.GetCoin((int)coin.Amount); // コインを加算
            Destroy(other.gameObject);
        }
    }

    // 落下死
    void DeadFall()
    {
        if(transform.position.y < -10f)
        {
            // ゲームオーバー シーン再ロード
            // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            Die(); // プレイヤーの死亡処理を呼び出す
        }
    }

    public void ReachGoal()
    {
        isGoal = true; // ゴールに到達したことを設定
    }

    public void Die()
    {
        isDead = true; // 死亡したことを設定
    }




#region Debug
    [ContextMenu("MorphDebug")]
    void MorphDebug()
    {
        Morph(-0.3f);

    }
#endregion
}
