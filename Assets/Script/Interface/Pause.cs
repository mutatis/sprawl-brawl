using UnityEngine;
using System.Collections;

public class Pause : MonoBehaviour 
{
	public void PauseGame(GameObject obj)
	{
		Time.timeScale = 0;
		obj.SetActive (true);
	}

	public void ResumeGame(GameObject obj)
	{
		Time.timeScale = 1;
		obj.SetActive (false);
	}

	public void Menu(string menu)
	{
		Time.timeScale = 1;
		Application.LoadLevel (menu);
	}
}
