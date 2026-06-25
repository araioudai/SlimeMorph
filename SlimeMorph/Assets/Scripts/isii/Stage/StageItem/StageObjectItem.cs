using UnityEngine;
using common;

public class StageObjectItem : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private float amount;
    [SerializeField] protected int mas;


    public int Id => id;
    public float Amount => amount;
    public int Mas => mas;

    public virtual void Init(StageObjectData data, float amount, int mas)
    {
        this.mas = mas;

        Debug.Log($"StageObjectItem Init: id={data.id}, type={data.type}, param={data.param}, amount={amount}, mas={mas}");


        this.id = data.id;
        float value = amount > 0 ? amount : data.param;

        switch (data.type)
        {
            case StageObjectType.Increase:
                // amountが0以下の場合はScriptableObjectの値を使用する
                this.amount = value;
                break;
            case StageObjectType.Decrease:
                // amountが0以下の場合はScriptableObjectの値を使用する
                this.amount = -value;
                break;
            default:
                break;
        }
    }
}
