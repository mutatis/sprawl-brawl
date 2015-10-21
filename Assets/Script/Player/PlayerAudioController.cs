using UnityEngine;
using System.Collections;

/* Toma conta dos sons do player */

public class PlayerAudioController : MonoBehaviour
{
    public AudioSource audios;

    public GameObject carrega;

    public AudioClip[] socoVento;
    public AudioClip[] soco;
    public AudioClip[] grito;

    public AudioClip chute;
    public AudioClip gritoChute;
    public AudioClip blockEnemy;
    public AudioClip sprawl;
    public AudioClip esquiva;

    bool isCarrega;
    bool obj;

    void Update()
    {
        if (PlayerController.player.velX > 0 && !audios.isPlaying)
        {
            audios.Play();
        }
        else if (PlayerController.player.velX == 0)
        {
            audios.Stop();
        }

        if(PlayerController.player.isEsquiva)
        {
            AudioSource.PlayClipAtPoint(esquiva, Camera.main.transform.position, 1f);
            PlayerController.player.isEsquiva = false;
        }

        if(Input.GetKeyUp(KeyCode.RightArrow))
        {
            Destroy(GameObject.FindGameObjectWithTag("Carrega"));
        }
    }

    //inimigo defendeu
    public void BlockEnemy()
    {
        AudioSource.PlayClipAtPoint(blockEnemy, Camera.main.transform.position, 1f);
    }

    public void PrimeiroSoco()
    {
        if (PlayerController.player.obj != null)
        {
            obj = true;
            AudioSource.PlayClipAtPoint(soco[Random.Range(0, soco.Length)], Camera.main.transform.position, 0.5f);
        }
        else
        {
            obj = false;
            AudioSource.PlayClipAtPoint(socoVento[Random.Range(0, socoVento.Length)], Camera.main.transform.position, 0.5f);
        }
        isCarrega = false;
    }

    //som de soco
    public void Soco()
    {
        if (obj)
        {
            AudioSource.PlayClipAtPoint(soco[Random.Range(0, soco.Length)], Camera.main.transform.position, 1f);
        }
        else
        {
            AudioSource.PlayClipAtPoint(socoVento[Random.Range(0, socoVento.Length)], Camera.main.transform.position, 1f);
        }
        Grito();
        isCarrega = false;
    }

    public void Grito()
    {
        AudioSource.PlayClipAtPoint(grito[Random.Range(0, grito.Length)], Camera.main.transform.position, 1f);
    }

    //som de socoForte
    public void SocoForte()
    {
        Destroy(GameObject.FindGameObjectWithTag("Carrega"));
        if (PlayerController.player.obj != null)
        {
            AudioSource.PlayClipAtPoint(chute, Camera.main.transform.position, 1f);
        }
        else
        {
            AudioSource.PlayClipAtPoint(socoVento[Random.Range(0, socoVento.Length)], Camera.main.transform.position, 1f);
        }
        AudioSource.PlayClipAtPoint(gritoChute, Camera.main.transform.position, 0.3f);
        isCarrega = false;
    }
    
    //som quando carrega soco forte
    public void Carrega()
    {
        if(!isCarrega)
        {
            Instantiate(carrega);
            isCarrega = true;
        }
    }

    //som do sprawl
    public void Sprawl()
    {
        if (PlayerController.player.esquiva)
        {
            AudioSource.PlayClipAtPoint(sprawl, Camera.main.transform.position, 1f);
        }
    }
}