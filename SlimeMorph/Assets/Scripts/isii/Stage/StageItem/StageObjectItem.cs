using UnityEngine;
using common;

public class StageObjectItem : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private float amount;

    public int Id => id;
    public float Amount => amount;

    public virtual void Init(StageObjectData data, float amount)
    {
        this.id = data.id;
        switch (data.type)
        {
            case StageObjectType.Increase:
                // コインの価値をparamから取得
                this.amount = amount;
                break;
            case StageObjectType.Decrease:
                // 減少の値をparamから取得
                this.amount = -amount;
                break;
            default:
                break;
        }
    }
}
