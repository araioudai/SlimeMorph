using UnityEngine;

public class StageCoin : StageObjectItem
{
    [SerializeField] int coinValue = 1; // コインの価値
    [SerializeField] float coinSpeed = 100f; // コインの回転速度\

    void Update()
    {
        // コインを回転させる
        transform.eulerAngles += new Vector3(0, coinSpeed * Time.deltaTime, 0);
    }
}
