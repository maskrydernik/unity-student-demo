// Yari_Retry.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class Yari_Retry : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
