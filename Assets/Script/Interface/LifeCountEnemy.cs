using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/* Mostra a vida do inimigo que esta em combate no momento */

public class LifeCountEnemy : MonoBehaviour
{
	public Text text;
	
	void Update ()
	{
		//mostra a vida do inimigo que vc ganho
		text.text = "Enemy Life: " + PlayerController.player. lifeEnemy.ToString();
	}
}