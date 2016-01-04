using UnityEngine;
using System.Collections;

public class TempSuperPunch : MonoBehaviour
{
    public GameObject obj;

    public void PlayGame()
    {
        Time.timeScale = 1;
        Destroy(obj);
    }
}
