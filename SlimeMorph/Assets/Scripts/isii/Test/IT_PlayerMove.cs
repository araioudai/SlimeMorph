using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IT_PlayerMove : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject player;
    [SerializeField] float flickSpeedMax = 10f;
    [SerializeField] float mouseSensitivity = 0.5f; // 要調整
    [SerializeField] float smoothing = 20f;         // 追従・止まる時の滑らかさ


    [Header("Lv")]
    private const string SelectedGrowKey = "SavedSelectedGrowIndex";

    int speedUpValue = 1; // スピードアップの倍率
    float percentSpeedUpValue = 1.5f; // スピードアップの倍率をパーセントで表す値

    private Vector3 lastMousePosition;
    private float accumulatedDeltaX = 0f;
    private float accumulatedTime = 0f;
    private float smoothedVelocity = 0f;

    private float currentVelocity = 0f;
    private bool isDragging = false;

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

        // ポインター（マウスやタッチ）が存在しない場合は処理しない
        if (Pointer.current == null) return;

        // タップ/クリック開始時（UI上でなければドラッグ開始）
        if (Pointer.current.press.wasPressedThisFrame)
        {
            if (!IsPointerOverUI())
            {
                isDragging = true;
            }
        }

        // 離した時
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        float targetVelocity = 0f;

        // ドラッグ中の目標速度計算
        if (isDragging && Pointer.current.press.isPressed)
        {
            // Pointer.current.delta から 1フレームあたりの移動量を直接取得
            float deltaX = Pointer.current.delta.x.ReadValue();
            targetVelocity = deltaX * mouseSensitivity;
        }

        // 毎フレームLerpで目標速度に補間（指を止めると0へスムーズに減衰する）
        currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.deltaTime * smoothing);

        float velocity = currentVelocity * percentSpeedUpValue;
        velocity = Mathf.Clamp(velocity, -flickSpeedMax, flickSpeedMax);

        Vector3 move = new Vector3(velocity, 0, 0) * Time.deltaTime;

        if (player != null)
        {
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

    /// <summary>
    /// タップ位置がボタンの上にあるかを判定する関数
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        //タップ/クリック位置を取得
        Vector2 pointerPosition = Vector2.zero;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Pointer.current != null)
        {
            pointerPosition = Pointer.current.position.ReadValue();
        }
        else
        {
            return false;
        }

        //ポインター位置にあるUIオブジェクトをすべて取得
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = pointerPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        //重なっているUIの中に「Button」が含まれているか判定
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Button>() != null)
            {
                return true; //ボタンの上なのでジャンプを無効化
            }
        }

        return false; //ボタンの上ではない
    }
}