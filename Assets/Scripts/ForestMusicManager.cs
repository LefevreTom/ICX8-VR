using UnityEngine;

public class ForestMusicManager : MonoBehaviour
{
    public AudioSource forestMusic;    // 森林背景音乐
    public AudioSource windSound;      // 新增：风声

    void Start()
    {
        // 确保开始时所有声音关闭
        if (forestMusic != null)
            forestMusic.Stop();
        if (windSound != null)         // 新增
            windSound.Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家进入森林，开始播放音效");
            if (forestMusic != null && !forestMusic.isPlaying)
            {
                forestMusic.Play();
            }
            if (windSound != null && !windSound.isPlaying)  // 新增
            {
                windSound.Play();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家离开森林，停止音效");
            if (forestMusic != null && forestMusic.isPlaying)
            {
                forestMusic.Stop();
            }
            if (windSound != null && windSound.isPlaying)   // 新增
            {
                windSound.Stop();
            }
        }
    }
}