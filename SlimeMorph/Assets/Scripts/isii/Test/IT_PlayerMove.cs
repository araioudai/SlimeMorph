using UnityEngine;

public class IT_PlayerMove : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject player;
    [SerializeField] float flickSpeedMax = 10f;

    // フリック操作でプレイヤーを移動させる
    void Update()
    {
        // if (Input.touchCount > 0)
        // {
        //     Touch touch = Input.GetTouch(0);
        //     if (touch.phase == TouchPhase.Moved)
        //     {
        //         Vector2 deltaPosition = touch.deltaPosition;
        //         float flickSpeed = deltaPosition.magnitude / touch.deltaTime;
        //         if (flickSpeed > flickSpeedMax)
        //         {
        //             flickSpeed = flickSpeedMax;
        //         }
        //         Vector3 moveDirection = new Vector3(deltaPosition.x, 0, 0).normalized;

        //         player.transform.Translate(moveDirection * flickSpeed * Time.deltaTime, Space.World);
        //     }
        // }

        // PCではマウスのドラッグでプレイヤーを移動させる 上記のフリック操作と同じような挙動にする
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            float dragSpeed = mouseDelta.magnitude / Time.deltaTime;
            if (dragSpeed > flickSpeedMax)
            {
                dragSpeed = flickSpeedMax;
            }
            Vector3 moveDirection = new Vector3(mouseDelta.x, 0, 0).normalized;

            player.transform.Translate(moveDirection * dragSpeed * Time.deltaTime, Space.World);
        }
    }
}
