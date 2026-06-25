using UnityEngine;
using UnityEngine.EventSystems;

public class UIObjectRotator : MonoBehaviour, IDragHandler
{
    [Header("回転させたいスライムの親オブジェクト")]
    [SerializeField] private Transform targetTransform;

    [Header("回転スピードの調整")]
    [SerializeField] private float rotationSpeed = 0.4f;

    /// <summary>
    /// スワイプされている間、呼ばれる関数
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (targetTransform == null) { return; }

        //eventData.delta.x で指が横にどれくらい動いたかを取得
        //右スワイプで右回転、左スワイプで左回転するように調整
        float rotationY = -eventData.delta.x * rotationSpeed;

        //スライムをY軸を中心に回転
        targetTransform.Rotate(Vector3.up, rotationY, Space.World);
    }
}