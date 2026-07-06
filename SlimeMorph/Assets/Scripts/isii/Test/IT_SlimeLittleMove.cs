using UnityEngine;

public class IT_SlimeLittleMove : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (IT_GameManager.Instance.isGoal) return; // ゴールに到達している場合は移動しない
        

        transform.Translate(Vector3.forward * Time.deltaTime * 0.1f);

        transform.localPosition = Vector3.zero;
    }
}
