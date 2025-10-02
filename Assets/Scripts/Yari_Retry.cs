// Yari_Retry.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class Yari_Retry : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadSceneAsync(currentScene.buildIndex);
        }
    }
}
