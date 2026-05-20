using UnityEngine;

public class IT_Coin : MonoBehaviour
{
    [SerializeField] int coinValue = 1; // コインの価値
    [SerializeField] float coinSpeed = 100f; // コインの回転速度\

    Transform playerTransform; // プレイヤーの位置を取得するための変数

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // コインを回転させる
        transform.eulerAngles += new Vector3(0, coinSpeed * Time.deltaTime, 0);
    }
}
