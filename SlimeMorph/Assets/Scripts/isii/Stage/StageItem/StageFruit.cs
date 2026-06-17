using UnityEngine;

public class StageFruit : StageObjectItem
{    
    bool isMorphed = false;
    [SerializeField] AudioClip buffSound; // バフ音
    [SerializeField] AudioClip debuffSound; // デバフ音

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isMorphed)
        {
            other.gameObject.GetComponent<IT_Player>().Morph(Amount);
            isMorphed = true;
            // バフ音またはデバフ音を再生
            if (Amount > 0 && buffSound != null)
                SoundManager.Instance.PlaySE(buffSound);
            else if (Amount < 0 && debuffSound != null)
                SoundManager.Instance.PlaySE(debuffSound);
            this.gameObject.SetActive(false); // オブジェクトを非表示にする
        }
    }
}
