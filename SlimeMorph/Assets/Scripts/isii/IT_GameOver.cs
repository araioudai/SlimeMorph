using UnityEngine;

public class IT_GameOver : MonoBehaviour
{
    Collider collider;

    void Start()
    {
        collider = GetComponent<Collider>();
    }

    void Update()
    {
        // プレイヤーが触れたらゲームオーバー シーン再ロード
        if (collider.bounds.Intersects(GameObject.Find("Player").GetComponent<Collider>().bounds))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
