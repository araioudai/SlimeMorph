using UnityEngine;

public class IT_PlayerMove : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject player;
    [SerializeField] float flickSpeedMax = 10f;


    [Header("Lv")]
    private const string SelectedGrowKey = "SavedSelectedGrowIndex";

    int speedUpValue = 1; // スピードアップの倍率
    float percentSpeedUpValue = 1.5f; // スピードアップの倍率をパーセントで表す値

    void Start()
    {
        // ゲーム開始時にPlayerPrefsから選択された成長タイプのインデックスを取得
        speedUpValue = PlayerPrefs.GetInt(SelectedGrowKey, 0);

        percentSpeedUpValue = 1f + (speedUpValue * 0.01f); // スピードアップの倍率をパーセントで表す値を計算
    }




    // フリック操作でプレイヤーを移動させる
    void Update()
    {
        if (IT_GameManager.Instance.isGoal) return;


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

        // // PCではマウスのドラッグでプレイヤーを移動させる 上記のフリック操作と同じような挙動にする
        if (Input.GetMouseButton(0))
        {
            // float mouseX = Input.GetAxis("Mouse X");
            // float dragSpeed = Mathf.Abs(mouseX) / Time.deltaTime;
            // if (dragSpeed > flickSpeedMax)
            // {
            //     dragSpeed = flickSpeedMax;
            // }
            // Vector3 moveDirection = new(Mathf.Sign(mouseX), 0, 0);
            // Vector3 move = moveDirection * dragSpeed * Time.deltaTime;

            // if (player.TryGetComponent<Rigidbody>(out var rb) && !rb.isKinematic)
            // {
            //     rb.MovePosition(rb.position + move);
            // }
            // else
            // {
            //     player.transform.Translate(move, Space.World);
            // }
            
            // 横移動のみ取得
            float mouseX = Input.GetAxis("Mouse X");
            Vector3 moveDirection = new Vector3(mouseX, 0, 0).normalized;
            float dragSpeed = Mathf.Abs(mouseX) / Time.deltaTime;

            dragSpeed *= percentSpeedUpValue; // 強化によるドラッグ速度上昇

            if (dragSpeed > flickSpeedMax)
            {
                dragSpeed = flickSpeedMax;
            }

            Vector3 move = moveDirection * dragSpeed * Time.deltaTime;
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
