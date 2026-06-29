using UnityEngine;

public class IT_Morph : MonoBehaviour
{
    [SerializeField] float morphSize = 1f;
    bool isMorphed = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isMorphed)
        {
            other.gameObject.GetComponent<IT_Player>().Morph(morphSize);
            isMorphed = true;
            this.gameObject.SetActive(false); // オブジェクトを非表示にする
        }
    }
}
