using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderForest : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadSceneAsync("Hub", LoadSceneMode.Additive);
    }
}
