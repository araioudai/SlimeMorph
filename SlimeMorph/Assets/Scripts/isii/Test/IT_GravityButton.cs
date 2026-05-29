using UnityEngine;

public class IT_GravityButton : MonoBehaviour
{
    enum CompareType
    {
        [InspectorName("以上")] GreaterThanOrEqual,
        [InspectorName("以下")] LessThanOrEqual
    }

    [SerializeField] GameObject setObject;
    [SerializeField] int setGravityValue = 3;
    bool isActivated = false;
    [SerializeField] CompareType switch_on;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isActivated)
        {
            switch (switch_on)
            {
                case CompareType.GreaterThanOrEqual:
                    if(other.gameObject.GetComponent<IT_Player>().GravityValue >= setGravityValue)
                    {
                        setObject.SetActive(false);
                        transform.localPosition -= new Vector3(0, 0.9f, 0);
                        isActivated = true;
                    }
                    break;
                case CompareType.LessThanOrEqual:
                    if(other.gameObject.GetComponent<IT_Player>().GravityValue <= setGravityValue)
                    {
                        setObject.SetActive(false);
                        transform.localPosition -= new Vector3(0, 0.9f, 0);
                        isActivated = true;
                    }
                    break;
                default:
                    break;
            }

        }
    }
}
