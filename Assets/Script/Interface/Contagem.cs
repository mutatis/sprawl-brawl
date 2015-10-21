using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Contagem : MonoBehaviour 
{
	public Text text;

	void Update()
	{
		text.text = "Ganho: " + GameMaster.master.vitorias;
	}
}
