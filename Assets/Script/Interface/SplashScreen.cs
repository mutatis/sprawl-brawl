using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour 
{
	public void Muda()
	{
        SceneManager.LoadScene("Menu");
	}
}
