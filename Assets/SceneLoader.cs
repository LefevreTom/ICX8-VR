using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadSceneAsync("Production", LoadSceneMode.Additive);
    }
}
