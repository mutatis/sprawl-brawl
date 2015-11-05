using UnityEngine;
using System.Collections;

public class Sombra : MonoBehaviour
{

    public Animator anim;

    public void Aumenta()
    {
        anim.SetTrigger("Sprawl");
    }
}