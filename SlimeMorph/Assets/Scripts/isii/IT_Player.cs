using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IT_Player : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    bool isMorphed = false;

    [SerializeField] Text coinText; // コインの数を表示するテキスト

    public int coinCount = 0; // コインの数をカウントする変数

    // Update is called once per frame
    void Update()
    {
        // 前方に移動
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        // カメラもプレイヤーと同じ速度で移動 ただしカメラは回転している為、プレイヤーの位置に合わせてカメラの位置を更新する
        Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z - 10f);
        Dead();
    }

    public void Morph(float morphSize)
    {
        if(isMorphed) return;
        isMorphed = true;
        // プレイヤーのサイズを+-変更
        transform.localScale += new Vector3(morphSize, 0, 0);
        StartCoroutine(MorphTimer());
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

}
