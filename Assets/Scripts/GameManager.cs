using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("��Ϸ��ʼ��");
    }

    public void StopAllDroneSounds()
    {
        DroneController[] drones = FindObjectsByType<DroneController>(FindObjectsSortMode.None);
        foreach (DroneController drone in drones)
        {
            if (drone != null)
            {
                drone.StopTalking(); // ���ڿ��Ե�����
            }
        }
        Debug.Log($"��ֹͣ {drones.Length} �����˻�������");
    }
}