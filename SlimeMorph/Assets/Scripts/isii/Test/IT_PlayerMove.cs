using UnityEngine;

public class IT_PlayerMove : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject player;
    [SerializeField] float flickSpeedMax = 10f;
    [SerializeField] float mouseSensitivity = 0.5f; // 要調整
    [SerializeField] float sampleWindow = 0.08f; // トラックパッド対策の平均化窓(要調整)


    [Header("Lv")]
    private const string SelectedGrowKey = "SavedSelectedGrowIndex";

    int speedUpValue = 1; // スピードアップの倍率
    float percentSpeedUpValue = 1.5f; // スピードアップの倍率をパーセントで表す値

    private Vector3 lastMousePosition;
    private float accumulatedDeltaX = 0f;
    private float accumulatedTime = 0f;
    private float smoothedVelocity = 0f;

    void Start()
    {
        // ゲーム開始時にPlayerPrefsから選択された成長タイプのインデックスを取得
        // speedUpValue = PlayerPrefs.GetInt(SelectedGrowKey, 0);

        int sideSpeedLv = PlayerPrefs.GetInt("GrowLevel_sidespeed_lv", 0);


        percentSpeedUpValue = 1f + (sideSpeedLv * 0.02f); // スピードアップの倍率をパーセントで表す値を計算
    }




    // フリック操作でプレイヤーを移動させる
    void Update()
    {
        if (IT_GameManager.Instance.isGoal || GameManager.Instance.GetPause()) return;


        // if (Input.touchCount > 0)
        // {
        //     Touch touch = Input.GetTouch(0);
        //     if (touch.phase == TouchPhase.Moved)
        //     {
        //         Vector2 deltaPosition = touch.deltaPosition;
        //         float flickSpeed = deltaPosition.magnitude / touch.deltaTime;
        //         if (flickSpeed > flickSpeedMax)
        //         {
        //             flickSpeed = flickSpeedMax;
        //         }
        //         Vector3 moveDirection = new Vector3(deltaPosition.x, 0, 0).normalized;

        //         player.transform.Translate(moveDirection * flickSpeed * Time.deltaTime, Space.World);
        //     }
        // }

        // PCではマウス/トラックパッドのドラッグでプレイヤーを移動させる
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            accumulatedDeltaX = 0f;
            accumulatedTime = 0f;
            smoothedVelocity = 0f;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition = Input.mousePosition;
            float deltaX = currentMousePosition.x - lastMousePosition.x;
            lastMousePosition = currentMousePosition;

            accumulatedDeltaX += deltaX;
            accumulatedTime += Time.deltaTime;

            // 一定時間分たまったら速度を更新(トラックパッドの入力ムラを平均化)
            if (accumulatedTime >= sampleWindow)
            {
                smoothedVelocity = (accumulatedDeltaX * mouseSensitivity) / accumulatedTime;
                accumulatedDeltaX = 0f;
                accumulatedTime = 0f;
            }

            float velocity = smoothedVelocity * percentSpeedUpValue; // 強化による速度上昇

            velocity = Mathf.Clamp(velocity, -flickSpeedMax, flickSpeedMax); // 速度(units/sec)としてクランプ

            Vector3 move = new Vector3(velocity, 0, 0) * Time.deltaTime; // 1フレーム分の移動量に戻す

            if (player.TryGetComponent<Rigidbody>(out var rb) && !rb.isKinematic)
            {
                rb.MovePosition(rb.position + move);
            }
            else
            {
                player.transform.Translate(move, Space.World);
            }
        }
    }
}