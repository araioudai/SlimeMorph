using UnityEngine;

public class StageMoveObstacle : StageObjectItem
{
    IT_PlayerPosCheck playerPosCheck;
    float speed = 10f; // 障害物の移動速度

    [Header("障害物が動き始めるマス")]
    [SerializeField] private int firstMoveMas = 3;
    [Header("当たり判定系統")]
    [SerializeField] AudioClip hitSound; // ヒット音
    bool isMorphed = false;


    void Start()
    {
        playerPosCheck = FindFirstObjectByType<IT_PlayerPosCheck>();

        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + firstMoveMas * 5);
        speed = FindFirstObjectByType<IT_Player>().Speed; // プレイヤーの移動速度を取得して障害物の移動速度に設定

    }

    // Update is called once per frame
    void Update()
    {
        if (playerPosCheck != null)
        {
            if (playerPosCheck.nowMas >= mas - firstMoveMas)
            {
                // ここで障害物を動かす処理を実装する
                // Debug.Log($"障害物が動きます。現在のマス: {playerPosCheck.nowMas}, 障害物のマス: {mas}");
                transform.Translate(Vector3.back * speed * Time.deltaTime);
            }
        }
        else
        {
            Debug.LogWarning("IT_PlayerPosCheckが見つかりません。");
        }

        if (transform.position.z < 0f) // 画面外に出たら削除
        {
            Destroy(gameObject);
        }


    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isMorphed)
        {
            other.gameObject.GetComponent<IT_Player>().Morph(Amount);
            isMorphed = true;
            // ヒット音を再生
            if (hitSound != null)
                SoundManager.Instance.PlaySE(hitSound);
        }
    }

}
