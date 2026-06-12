using UnityEngine;

public class StageRock : StageObjectItem
{    
    bool isMorphed = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isMorphed)
        {
            other.gameObject.GetComponent<IT_Player>().Morph(Amount);
            isMorphed = true;
        }
    }
}
