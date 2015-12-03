using UnityEngine;
using System.Collections;

public class Background : MonoBehaviour
{
    public static Background fundo;

    public Animator anim;

    void Awake()
    {
        fundo = this;
    }
    
    public void Play()
    {
        anim.SetTrigger("Nervoso");
    }    
}
