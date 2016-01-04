using UnityEngine;
using System.Collections;

public class BuySuperPunch : MonoBehaviour
{
    public GameObject obj;

    public int preco;

    public void Buy()
    {
        //     if(PlayerPrefs.GetInt("Sapatos") >= preco)
        //   {
        PlayerController.player.superPunch = true;
        //   PlayerPrefs.SetInt("Sapatos", preco * -1);
        Time.timeScale = 1;
        Destroy(obj);
        // }
    }
}
