using UnityEngine;

public class StageFruit : StageObjectItem
{
    bool isMorphed = false;
    [SerializeField] AudioClip buffSound; // バフ音
    [SerializeField] AudioClip debuffSound; // デバフ音
    [SerializeField] float rotationSpeed = 50f; // 回転速度

    void Update()
    {
        if (isStop) return;
        // オブジェクトを回転させる y軸を中心に回転させる
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isMorphed)
        {
            if (!other.TryGetComponent<IT_Player>(out var player))
                player = other.GetComponentInParent<IT_Player>();

            if (player == null)
            {
                Debug.LogWarning($"StageFruit: IT_Player not found on trigger target {other.name}", other);
                return;
            }

            player.Morph(Amount);
            isMorphed = true;
            // バフ音またはデバフ音を再生
            if (Amount > 0 && buffSound != null && SoundManager.Instance != null)
                SoundManager.Instance.PlaySE(buffSound);
            else if (Amount < 0 && debuffSound != null && SoundManager.Instance != null)
                SoundManager.Instance.PlaySE(debuffSound);
            this.gameObject.SetActive(false); // オブジェクトを非表示にする
        }
    }
}
