using UnityEngine;
using System.Collections;

public class SomBotao : MonoBehaviour
{
    public void Click(AudioClip click)
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            AudioSource.PlayClipAtPoint(click, transform.position, 1f);
            Time.timeScale = 0;
        }
        else
        {
            AudioSource.PlayClipAtPoint(click, transform.position, 1f);
        }
    }
}