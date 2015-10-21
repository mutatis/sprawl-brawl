using UnityEngine;
using System.Collections;

public class SplashScreen : MonoBehaviour 
{

	void Start()
	{
		Muda ();
	}

	public void Muda()
	{
		Application.LoadLevel("Menu");
	}
}
