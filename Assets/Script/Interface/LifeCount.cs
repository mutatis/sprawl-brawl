using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LifeCount : MonoBehaviour
{
    public Text text;
	
	void Update ()
    {
        text.text = "Life: " + PlayerController.player. life.ToString();
	}
}