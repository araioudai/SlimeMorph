using UnityEngine;
using common;
using System.IO;

public class StageRock : StageObjectItem
{
    bool isMorphed = false;
    [SerializeField] AudioClip hitSound; // ヒット音

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isMorphed)
        {
            other.gameObject.GetComponent<IT_Player>().Morph(Amount);
            isMorphed = true;
            // ヒット音を再生
            // if (hitSound != null)
            //     SoundManager.Instance.PlaySE(hitSound);
            SoundManager.Instance.PlaySE(SE.HitDamage);
        }
    }
}
