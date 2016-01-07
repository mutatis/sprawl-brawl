using UnityEngine;
using System.Collections;

/* Codigo q toma conta do estado do jogo, dinheiro etc */

public class GameMaster : MonoBehaviour 
{
	public static GameMaster master;
    public AudioSource audio;

    public GameObject revive;
    public GameObject player;

	[HideInInspector]
	public int vitorias;
    public int tipo;

	void Awake()
	{
		master = this;
        Time.timeScale = 1;
    }

   /* void Start()
    {
        Time.timeScale = 0;
    }*/

    void Update()
    {
        if(PlayerController.player.life < 0 && Time.timeScale > 0)
        {
            Morreu();
        }
        /*if (Time.timeScale == 0 && audio.volume > 0.3f)
        {
            audio.volume -= 0.01f;
        }
        else if(Time.timeScale == 1 && audio.volume < 1)
        {
            audio.volume += 0.01f;
        }*/
    }

    public void Morreu()
    {
        Instantiate(revive);
        Time.timeScale = 0;
    }

    public void Revive()
    {
        Vector3 temp = PlayerController.player.transform.position;
        Destroy(PlayerController.player.obj);
        Destroy(PlayerController.player.gameObject);
        Instantiate(player, temp, transform.rotation);
        Time.timeScale = 1;
    }
}
