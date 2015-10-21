using UnityEngine;
using System.Collections;

public class CriaCriador : MonoBehaviour 
{
    public SpriteRowCreator creator;

    void OnTriggerEnter2D(Collider2D other)
    {
        //se bateu no player cria sprite com outros inimigos
        if (other.gameObject.tag == "Player")
        {
            creator.CreateSprites();
        }
    }
}
