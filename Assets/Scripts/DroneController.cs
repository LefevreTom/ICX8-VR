using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class DroneController : MonoBehaviour
{
    [Header("无人机设置")]
    public string droneName = "无人机";
    public float activationDistance = 15f;
    public float clickDistance = 5f;

    [Header("声音设置")]
    public AudioClip bbSound;          // BB声音
    public AudioClip talkingSound;     // 说话声音

    [Header("UI链接")]
    public GameObject dialogPanel;
    public Text dialogText;

    [Header("浮动动画设置 (Floating Animation)")]
    public float floatAmplitude = 0.25f;     // How high to float
    public float floatFrequency = 1.5f;      // Speed of floating

    [Header("朝向玩家设置 (Look At Player)")]
    public float lookAtSpeed = 4f; // How fast the drone rotates toward the player

    private AudioSource audioSource;
    private Transform player;
    private bool isActive = false;
    private bool isTalking = false;
    private bool wasInteracted = false; // 是否已经交互过
    private bool wasInRange = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Transform vrCamera;
    private bool alreadyActivated = false;
    public bool droneToReveal = false;

    void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.rotation;
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        if (droneToReveal)
            gameObject.SetActive(false);
    }
    void Start()
    {
        // 获取或添加AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D音效
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = activationDistance;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        var xr = FindObjectsByType<XROrigin>(FindObjectsSortMode.None);
        vrCamera = Camera.main.transform;
    }

    void Update()
    {
        if (player == null) return;
        ApplyFloatingMotion();


        float distance = Vector3.Distance(transform.position, player.position);
        LookAtPlayer(distance);

        // ------------------------------
        // PLAYER WITHIN ACTIVATION RANGE
        // ------------------------------
        if (distance <= activationDistance)
        {
            if (!isActive)
                ActivateDrone();

            // Show popup UI automatically when close enough
            if (distance <= clickDistance && !dialogPanel.activeSelf && !isTalking && !wasInRange)
            {
                dialogPanel.SetActive(true);
                StartTalking();
                wasInRange = true;
            }

            // Fade BB sound as player approaches
            if (!wasInteracted && isActive && !isTalking && audioSource.isPlaying)
            {
                float volume = 1f - (distance / activationDistance);
                audioSource.volume = Mathf.Clamp(volume, 0.1f, 0.8f);
            }

            // ------------------------------
            // CLICK INTERACTION
            // ------------------------------
            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (!isTalking && distance <= clickDistance)
                {
                    StartTalking();
                    dialogPanel.SetActive(true);
                }
                else if (isTalking)
                {
                    StopTalking();
                    dialogPanel.SetActive(false);
                }
            }
        }
        // ------------------------------
        // PLAYER LEFT THE RANGE
        // ------------------------------
        else
        {
            if (isActive)
                DeactivateDrone();
                wasInRange = false;

            if (dialogPanel.activeSelf)
                dialogPanel.SetActive(false);
        }
    }


    void ActivateDrone()
    {
        isActive = true;

        // 播放BB声音（如果还没交互过）
        if (!wasInteracted && bbSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = bbSound;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.Play();
        }
    }

    void DeactivateDrone()
    {
        isActive = false;

        // 停止声音（如果不是在说话状态）
        if (isTalking)
        {
            StopTalking();
        }
    }

    void StartTalking()
    {
        // 停止BB声音
        audioSource.Stop();

        // 播放说话声音
        if (talkingSound != null)
        {
            audioSource.clip = talkingSound;
            audioSource.loop = true;
            audioSource.volume = 0.8f;
            audioSource.Play();
            Debug.Log(droneName + "开始说话");
        }

        // 显示对话框
        if (dialogPanel != null && dialogText != null)
        {
            //dialogText.text = droneName ;
            dialogPanel.SetActive(true);
        }

        isTalking = true;
        wasInteracted = true; // 标记为已交互
    }

    public void StopTalking()
    {
        // 停止所有声音
        audioSource.Stop();

        // 隐藏对话框
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        isTalking = false;
    }

    // 在Scene视图中显示检测范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, clickDistance);
    }

    void ApplyFloatingMotion()
    {
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        // Only modify Y, keep x/z untouched
        transform.localPosition = new Vector3(
            initialPosition.x,
            newY,
            initialPosition.z
        );
    }

    void LookAtPlayer(float distance)
    {
        if (vrCamera == null) return;
        // If within activation range → look at player
        if (distance <= activationDistance)
        {
            Vector3 direction = vrCamera.position - transform.position;

            if (direction.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * lookAtSpeed
            );
        }
        else
        {
            // Player is far away → return to original idle rotation
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                initialRotation,
                Time.deltaTime * (lookAtSpeed * 0.5f) // slower for a natural "idle" feel
            );
        }
    }


    public void Activate()
    {
        if (alreadyActivated) return; // run only once
        alreadyActivated = true;
        if (droneToReveal)
            gameObject.SetActive(true);
    }

}