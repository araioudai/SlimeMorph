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

    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Player"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
