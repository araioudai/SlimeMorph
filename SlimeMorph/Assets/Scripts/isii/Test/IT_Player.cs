using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;

public class IT_Player : StageObjectItem
{
    [Header("移動速度")]
    [SerializeField] float speed = 5f;
    public float Speed { get { return speed; } } // 移動速度を外部から取得できるようにするプロパティ
    bool isMorphed = false;
    [Header("時間経過で減るサイズの量")]
    [SerializeField] float timeMorphDown = 1f; // 時間経過で減るサイズの量

    [Header("重力の値")]
    [SerializeField] float gravityValue = 3; // 重力の値
    public float GravityValue { get { return gravityValue; } } // 重力の値を外部から取得できるようにするプロパティ

    [Header("プレイヤーのサイズ")]
    public Vector3 PlayerSize { get { return slime.transform.localScale; } } // プレイヤーのサイズを外部から取得できるようにするプロパティ


    [Header("カメラとの距離")]
    [SerializeField] float cameraDistance = 3f; // カメラとの距離

    [Header("時間経過でサイズを減らすかどうか")]
    [SerializeField] bool isTimeMorphDown = true; // 時間経過でサイズを減らすかどうか

    [Header("地面追従")]
    [SerializeField] float groundProbeOffset = 0.5f;
    [SerializeField] float groundProbeDistance = 1.8f;
    [SerializeField] float groundSnapOffset = 0.05f;
    [SerializeField] float maxGroundAngle = 80f;
    [SerializeField] LayerMask groundLayerMask = ~0;
    [SerializeField] string groundTag = "Ground";

    bool isGoal = false; // ゴールに到達したかどうかを管理するフラグ
    bool isDead = false; // 死亡しているかどうかを管理するフラグ

    [Header("Slime")]
    GameObject slime;
    public Rigidbody rb;



    [Header("強化")]
    float defenseValue = 0.5f; // 防御力の倍率
    float decreaseValue = 0.5f; // 減少量の倍率

    float percentDefenseValue = 0.5f; // 防御力の倍率
    float percentDecreaseValue = 0.5f; // 減少量の倍率



    // [Header("Lv")]
    // private const string SelectedGrowKey = "SavedSelectedGrowIndex";




    // 0 = スピードアップ, 1 = 防御力アップ, 2 = 減少量ダウン


    public void SpeedReset()
    {
        speed = 0f;
    }

    public void DawnFall()
    {
        if (rb == null) return;

        rb.AddForce(Vector3.down * 1000, ForceMode.Acceleration);
        Debug.Log("DawnFall: Added downward force to the player.");
    }



    void Test()
    {
        //他のステータス情報もPlayerPrefsから取得
        // int sideSpeedLv = PlayerPrefs.GetInt("GrowLevel_sidespeed_lv", 0);
        int defenseLv = PlayerPrefs.GetInt("GrowLevel_defence_lv", 0);
        int shrinkLv = PlayerPrefs.GetInt("GrowLevel_shrink_lv", 0);
        // int clearStage = PlayerPrefs.GetInt("ClearStage", 1);





        // defenseValue = PlayerPrefs.GetInt(SelectedGrowKey, 1);
        // decreaseValue = PlayerPrefs.GetFloat(SelectedGrowKey, 2);

        percentDefenseValue = 1f - (defenseLv * 0.01f); // 防御力の倍率をパーセントで表す値を計算
        percentDecreaseValue = 1f - (shrinkLv * 0.01f); // 減少量の倍率をパーセントで表す値を計算
    }




    bool isStart = false; // ゲーム開始時のフラグ

    public void Init()
    {
        IT_GameManager.Instance.RegisterStageObject(this); // IT_GameManagerにプレイヤーを登録
        slime = gameObject.transform.GetChild(0).gameObject; // Slimeオブジェクトを取得
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        isStart = true; // ゲーム開始時のフラグをtrueに設定
Test();
    }

    void Update()
    {
        if (!isStart) return; // ゲーム開始前は処理しない
        if (isGoal) return; // ゴールに到達している場合は移動しない
        if (isDead) return; // 死亡している場合は移動しない
        if (isStop) return; // 停止している場合は移動しない

        // カメラもプレイヤーと同じ速度で移動 ただしカメラは回転している為、プレイヤーの位置に合わせてカメラの位置を更新する
        // Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z - cameraDistance);
        
        // transform.Translate(speed * Time.deltaTime * Vector3.forward);
        
        Dead();
        // DeadFall();
        MorphDown();
    }

    private void FixedUpdate()
    {
        if (isGoal) return;
        if (isDead) return;
        if (isStop) return;
        if (rb == null) return;

        if (TryGetGroundHit(out RaycastHit hit))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle <= maxGroundAngle)
            {
                Vector3 forwardOnGround = Vector3.ProjectOnPlane(Vector3.forward, hit.normal);
                if (forwardOnGround.sqrMagnitude <= 0.0001f)
                {
                    forwardOnGround = Vector3.forward;
                }
                forwardOnGround.Normalize();

                Vector3 nextPos = rb.position + forwardOnGround * speed * Time.fixedDeltaTime;
                nextPos.y = hit.point.y + groundSnapOffset;

                rb.MovePosition(nextPos);
                rb.linearVelocity = Vector3.zero;
                return;
            }
        }

        // 接地していないときは落下しつつ前進
        Vector3 velocity = rb.linearVelocity;
        velocity.x = 0f;
        velocity.z = speed;
        rb.linearVelocity = velocity;

        // Dead();
        // DeadFall();
        MorphDown();


    }

    private bool TryGetGroundHit(out RaycastHit hit)
    {
        Vector3 origin = rb.position + Vector3.up * groundProbeOffset;
        if (!Physics.Raycast(origin, Vector3.down, out hit, groundProbeDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return hit.collider != null && hit.collider.CompareTag(groundTag);
    }

    public void Morph(float morphSize)
    {
        if(isMorphed) return;
        isMorphed = true;
        // プレイヤーのサイズを+-変更

        // 減少だった場合
        if(morphSize < 0)
        {
            // decreaseValueを掛けて減少量を調整
            morphSize *= percentDefenseValue;
        }

        slime.transform.localScale += new Vector3(morphSize, morphSize, morphSize);
        slime.GetComponent<BoxCollider>().size += new Vector3(morphSize, morphSize, morphSize); // コライダーのサイズも変更
        if (morphSize < 0)
            gravityValue -= 1;
        else
            gravityValue += 1;
        StartCoroutine(MorphTimer());
        Debug.Log($"Morph: {morphSize}");
    }

    void MorphDown()
    {
        if (!isTimeMorphDown)
        {
            Debug.Log("MorphDown: isTimeMorphDown is false, skipping MorphDown.");
            return;
        }
        // 時間経過でサイズを減らす

        float morphAmount = timeMorphDown * percentDecreaseValue * Time.deltaTime;

        slime.transform.localScale -= new Vector3(morphAmount, morphAmount, morphAmount);
        slime.GetComponent<BoxCollider>().size -= new Vector3(morphAmount, morphAmount, morphAmount); // コライダーのサイズも変更
        gravityValue -= morphAmount;

        //Debug.Log($"MorphDown: {morphAmount}, New Scale: {slime.transform.localScale}");
    }

    IEnumerator MorphTimer()
    {
        yield return new WaitForSeconds(0.5f);
        isMorphed = false;
    }

    void Dead()
    {
        if(slime.transform.localScale.x <= 0.01f)
        {
            // ゲームオーバー シーン再ロード
            Die(); // プレイヤーの死亡処理を呼び出す
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.TryGetComponent(out StageCoin coin))
    //     {
    //         IT_GameManager.Instance.GetCoin((int)coin.Amount); // コインを加算

    //         Destroy(other.gameObject);
    //     }
    // }

    // 落下死
    void DeadFall()
    {
        if(slime.transform.position.y < -10f)
        {
            // ゲームオーバー シーン再ロード
            // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            Die(); // プレイヤーの死亡処理を呼び出す
        }
    }

    public void ReachGoal()
    {
        StopMovement();
        isGoal = true; // ゴールに到達したことを設定
    }

    public void Die()
    {
        StopMovement();

        isDead = true; // 死亡したことを設定
    }

    private void StopMovement()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }




#region Debug
    [ContextMenu("MorphDebug")]
    void MorphDebug()
    {
        Morph(-0.3f);

    }

#endregion


}
