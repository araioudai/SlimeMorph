using UnityEngine;

public class ObjectRotate : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f; // 回転速度




    void Update()
    {
        // オブジェクトを回転させる y軸を中心に回転させる
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
    }

}
