using UnityEngine;
using System.Collections;

public class LoadScene : MonoBehaviour 
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Application.LoadLevel("Playing");
        }
    }

	public void Loading(string nome)
	{
		Application.LoadLevel (nome);
	}
}
