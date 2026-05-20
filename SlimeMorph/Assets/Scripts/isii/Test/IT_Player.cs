using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IT_Player : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    bool isMorphed = false;
    [SerializeField] float timeMorphDown = 1f; // 時間経過で減るサイズの量

    [SerializeField] float gravityValue = 3; // 重力の値
    public float GravityValue { get { return gravityValue; } } // 重力の値を外部から取得できるようにするプロパティ

    [SerializeField] Text coinText; // コインの数を表示するテキスト

    int coinCount = 0; // コインの数をカウントする変数
    public int CoinCount { get { return coinCount; } } // コインの数を外部から取得できるようにするプロパティ

    BoxCollider collider;
    void Start()
    {
        collider = GetComponent<BoxCollider>();
        coinText.text = "Coins: " + coinCount;
    }

    // Update is called once per frame
    void Update()
    {
        // 前方に移動
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        // カメラもプレイヤーと同じ速度で移動 ただしカメラは回転している為、プレイヤーの位置に合わせてカメラの位置を更新する
        Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z - 10f);
        Dead();
        DeadFall();
        MorphDown();
    }

    public void Morph(float morphSize)
    {
        if(isMorphed) return;
        isMorphed = true;
        // プレイヤーのサイズを+-変更
        transform.localScale += new Vector3(morphSize, 0, 0);
        collider.size += new Vector3(morphSize, 0, 0); // コライダーのサイズも変更
        if (morphSize < 0)
            gravityValue -= 1;
        else
            gravityValue += 1;
        StartCoroutine(MorphTimer());
    }

    void MorphDown()
    {
        // 時間経過でサイズを減らす
        transform.localScale -= new Vector3(timeMorphDown * Time.deltaTime, 0, 0);
        collider.size -= new Vector3(timeMorphDown * Time.deltaTime, 0, 0); // コライダーのサイズも変更
        gravityValue -= timeMorphDown * Time.deltaTime;
    }

    IEnumerator MorphTimer()
    {
        yield return new WaitForSeconds(1f);
        isMorphed = false;
    }

    void Dead()
    {
        if(transform.localScale.x <= 0.01f)
        {
            // ゲームオーバー シーン再ロード
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Coin"))
        {
            coinCount++;
            coinText.text = "Coins: " + coinCount;
            Destroy(other.gameObject);
        }
    }

    // 落下死
    void DeadFall()
    {
        if(transform.position.y < -10f)
        {
            // ゲームオーバー シーン再ロード
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

}
