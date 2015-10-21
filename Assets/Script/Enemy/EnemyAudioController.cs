using UnityEngine;
using System.Collections;

public class EnemyAudioController : MonoBehaviour
{
    public AudioClip takedown;

    bool tocaT;

    //som de takeDown so toca 1 vez mas a animacao toca varias vezes por isso o bool
    public void Takedown()
    {
        if (!tocaT)
        {
            AudioSource.PlayClipAtPoint(takedown, Camera.main.transform.position, 1f);
            tocaT = true;
        }
    }
}