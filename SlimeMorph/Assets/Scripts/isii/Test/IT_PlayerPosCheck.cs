using UnityEngine;

public class IT_PlayerPosCheck : MonoBehaviour
{
    public int nowMas = 0;
    Vector3 nowPos = Vector3.zero;

    void Start()
    {
        nowPos = transform.position;
    }

    void Update()
    {
        if (nowPos != transform.position)
        {
            nowPos = transform.position;
            CheckMas();
        }
    }

    void CheckMas()
    {
        nowMas = Mathf.FloorToInt((nowPos.z + 2.5f) / 5) + 1;
    }
}
