using UnityEngine;
using System.Collections;

public class BuyRevive : MonoBehaviour
{
    public GameObject obj;

    public int preco;

    public void Buy()
    {
   //     if(PlayerPrefs.GetInt("Sapatos") >= preco)
     //   {
            GameMaster.master.Revive();
        //   PlayerPrefs.SetInt("Sapatos", preco * -1);
            Destroy(obj);   
       // }
    }    
}
