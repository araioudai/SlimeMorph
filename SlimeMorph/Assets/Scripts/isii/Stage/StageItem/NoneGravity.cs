using UnityEngine;

public class NoneGravity : MonoBehaviour
{




    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out IT_Player itp))
            {
                itp.SpeedReset();
                itp.DawnFall();
                Debug.Log("SpeedReset");
            }
            else
            {
                Debug.LogWarning("IT_Playerコンポーネントが見つかりませんでした。");
            }
        }
    }





}
