using UnityEngine;
using System.Collections;

public class CarregandoManager : MonoBehaviour
{
    public AudioSource audios;

	void Update ()
    {
	    if(!audios.isPlaying)
        {
            Destroy(gameObject);
        }
	}
}