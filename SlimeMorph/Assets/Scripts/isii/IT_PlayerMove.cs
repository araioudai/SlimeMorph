using UnityEngine;

public class IT_PlayerMove : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject player;
    [SerializeField] float flickSpeedMax = 10f;

    // フリック操作でプレイヤーを移動させる
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 deltaPosition = touch.deltaPosition;
                float flickSpeed = deltaPosition.magnitude / touch.deltaTime;
                if (flickSpeed > flickSpeedMax)
                {
                    flickSpeed = flickSpeedMax;
                }
                Vector3 moveDirection = new Vector3(deltaPosition.x, 0, deltaPosition.y).normalized;
                player.transform.Translate(moveDirection * flickSpeed * Time.deltaTime);
            }
        }
    }
}
